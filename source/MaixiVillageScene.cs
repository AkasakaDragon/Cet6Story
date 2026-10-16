using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

/// <summary>A height-scaled village panorama with a movable, native-size hero.</summary>
public sealed class MaixiVillageScene : Control
{
    const float MovementSpeed = 160f;
    const double MaximumDelta = 0.05;
    const int ReturnButton = 0, QuestButton = 1, LeftButton = 2, RightButton = 3, MillButton = 4;

    // World pixels follow the visible stair treads; bridge feet rest on the deck, not its front beam.
    static readonly PointF[] GroundProfile = {
        new PointF(0, 506), new PointF(1790, 506),
        new PointF(1792, 490), new PointF(1815, 490),
        new PointF(1817, 477), new PointF(1838, 477),
        new PointF(1840, 464), new PointF(1860, 464),
        new PointF(1862, 452), new PointF(1882, 452),
        new PointF(1884, 439), new PointF(1905, 439),
        new PointF(1907, 427), new PointF(1925, 427),
        new PointF(1927, 420), new PointF(2575, 420),
        new PointF(2577, 437), new PointF(2599, 437),
        new PointF(2601, 450), new PointF(2622, 450),
        new PointF(2624, 464), new PointF(2647, 464),
        new PointF(2649, 477), new PointF(2671, 477),
        new PointF(2673, 490), new PointF(2694, 490),
        new PointF(2696, 506), new PointF(5933, 506)
    };
    static readonly float[] AreaStarts = {
        120, 250, 600, 1090, 1530, 2070, 2250, 2535,
        2760, 3310, 4100, 4710, 5200, 5750
    };
    static readonly string[] AreaNames = {
        "西侧村口", "救济告示板", "村中摊位", "石屋住宅", "农田与民居",
        "水车与磨坊", "水车下藏身处", "排水机关", "旧储水间",
        "救援空地", "牲畜围栏", "老人住宅", "村民交谈处", "东侧出口"
    };

    // Both images belong to the game's shared cache, not this control.
    readonly Image panorama;
    readonly Image hero;
    readonly Image[] walkFrames;
    bool ownsWalkFrames;
    public static MaixiVillageScene FromWalkGif(Image panorama, Image hero, string path)
    {
        Image[] frames = new Image[8];
        try {
            using (Image gif = Image.FromFile(path)) {
                var dimension = System.Drawing.Imaging.FrameDimension.Time;
                if (gif.Width != 136 || gif.Height != 152 || gif.GetFrameCount(dimension) != 8)
                    throw new ArgumentException("Walking GIF must contain eight 136 x 152 frames.", "path");
                for (int i = 0; i < frames.Length; i++) {
                    gif.SelectActiveFrame(dimension, i);
                    frames[i] = new Bitmap(gif);
                }
            }
            var scene = new MaixiVillageScene(panorama, hero, frames);
            scene.ownsWalkFrames = true;
            return scene;
        } catch {
            foreach (Image frame in frames) if (frame != null) frame.Dispose();
            throw;
        }
    }
    bool walking;
    double walkTime;
    internal int WalkFrameIndex { get { return (int)(walkTime * 10.0 + 0.000001) % 8; } }
    readonly System.Windows.Forms.Timer movementTimer;
    readonly HashSet<Keys> movementKeys = new HashSet<Keys>();
    readonly Rectangle[] buttonBounds = new Rectangle[5];
    Form ownerForm;
    float playerX, cameraX;
    bool facingLeft, actionInProgress, disposed, clearingInput, currentKeyIsRepeat;
    bool refocusAfterQuest;
    int pressedButton = -1, hoveredButton = -1, keyboardButton = -1;
    int keyboardHoldDirection;
    long lastTick;

    public Action Return;
    public Action OpenQuest;
    public Action OpenMill;
    internal void PlacePlayer(float x){playerX=Clamp(x,MinimumPlayerX,MaximumPlayerX);cameraX=Clamp(playerX-WorldViewportWidth*.45f,0,MaximumCameraX);Invalidate();}
    public float PlayerX { get { return playerX; } }
    public float CameraX { get { return cameraX; } }

    public MaixiVillageScene(Image panorama, Image hero)
        : this(panorama, hero, null) { }

    public MaixiVillageScene(Image panorama, Image hero, Image[] walkFrames)
    {
        if (panorama == null) throw new ArgumentNullException("panorama");
        if (hero == null) throw new ArgumentNullException("hero");
        this.panorama = panorama;
        this.hero = hero;
        if (walkFrames != null) {
            if (walkFrames.Length != 8) throw new ArgumentException("Eight walking frames required.", "walkFrames");
            foreach (Image frame in walkFrames)
                if (frame == null || frame.Width != 136 || frame.Height != 152)
                    throw new ArgumentException("Walking frames must be 136 x 152.", "walkFrames");
            this.walkFrames = (Image[])walkFrames.Clone();
        }
        playerX = Clamp(400f, MinimumPlayerX, MaximumPlayerX);

        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
            ControlStyles.Selectable, true);
        BackColor = Color.FromArgb(19, 35, 28);
        TabStop = true;
        AccessibleName = "麦溪村；A、D 或左右方向键移动；Tab 选择按钮，Enter 确认";
        AccessibleRole = AccessibleRole.Client;

        movementTimer = new System.Windows.Forms.Timer { Interval = 16 };
        movementTimer.Tick += MovementTimerTick;
        lastTick = Stopwatch.GetTimestamp();
    }

    float MapScale { get { return Height > 0 ? Height / (float)panorama.Height : 1f; } }
    float WorldViewportWidth { get { return Math.Max(0, Width) / MapScale; } }
    float MaximumCameraX { get { return Math.Max(0f, panorama.Width - WorldViewportWidth); } }
    float MinimumPlayerX { get { return Math.Min(panorama.Width * 0.5f, 24f + (walkFrames != null ? 70f : hero.Width * 0.5f)); } }
    float MaximumPlayerX { get { return panorama.Width - MinimumPlayerX; } }
    float MapOffsetX { get { return Math.Max(0f, (Width - panorama.Width * MapScale) * 0.5f); } }

    static float Clamp(float value, float minimum, float maximum)
    {
        return Math.Max(minimum, Math.Min(maximum, value));
    }

    public float GroundYAt(float worldX)
    {
        if (Single.IsNaN(worldX) || worldX <= GroundProfile[0].X) return GroundProfile[0].Y;
        for (int i = 1; i < GroundProfile.Length; i++) {
            PointF previous = GroundProfile[i - 1], next = GroundProfile[i];
            if (worldX > next.X) continue;
            float fraction = (worldX - previous.X) / (next.X - previous.X);
            return previous.Y + (next.Y - previous.Y) * fraction;
        }
        return GroundProfile[GroundProfile.Length - 1].Y;
    }

    bool CanAcceptMovement()
    {
        if (disposed || actionInProgress || !Visible || !Enabled || !ContainsFocus) return false;
        return ownerForm == null || (ownerForm.Enabled && Form.ActiveForm == ownerForm);
    }

    public void AdvanceMovement(double deltaSeconds)
    {
        if (!CanAcceptMovement()) {
            ClearInput();
            return;
        }
        bool left = movementKeys.Contains(Keys.Left) || movementKeys.Contains(Keys.A) ||
            pressedButton == LeftButton || keyboardHoldDirection < 0;
        bool right = movementKeys.Contains(Keys.Right) || movementKeys.Contains(Keys.D) ||
            pressedButton == RightButton || keyboardHoldDirection > 0;
        AdvanceMovementCore(deltaSeconds, (right ? 1 : 0) - (left ? 1 : 0));
    }

    // The timer uses the focus/modal guard above; tests can exercise geometry alone here.
    internal void AdvanceMovementCore(double deltaSeconds, int direction)
    {
        if (disposed || Double.IsNaN(deltaSeconds) || Double.IsInfinity(deltaSeconds)) return;
        double delta = Math.Max(0, Math.Min(MaximumDelta, deltaSeconds));
        int movement = Math.Sign(direction);
        float previousX = playerX, previousCamera = cameraX;
        bool wasWalking = walking;
        int previousFrame = WalkFrameIndex;
        if (movement != 0 && delta > 0) {
            facingLeft = movement < 0;
            playerX = Clamp(playerX + movement * MovementSpeed * (float)delta,
                MinimumPlayerX, MaximumPlayerX);
        }
        walking = playerX != previousX;
        if (walking) walkTime = wasWalking ? (walkTime + delta) % 0.8 : 0;
        else walkTime = 0;
        float desired = Clamp(playerX - WorldViewportWidth * 0.45f, 0f, MaximumCameraX);
        cameraX += (desired - cameraX) * (float)(1 - Math.Exp(-8 * delta));
        cameraX = Clamp(cameraX, 0f, MaximumCameraX);
        if (Math.Abs(desired - cameraX) < 0.05f) cameraX = desired;
        if (previousX != playerX || previousCamera != cameraX || wasWalking != walking || previousFrame != WalkFrameIndex) Invalidate();
    }

    void MovementTimerTick(object sender, EventArgs e)
    {
        long now = Stopwatch.GetTimestamp();
        double delta = (now - lastTick) / (double)Stopwatch.Frequency;
        lastTick = now;
        AdvanceMovement(delta);
    }

    void RefreshTimer()
    {
        if (movementTimer == null || disposed) return;
        lastTick = Stopwatch.GetTimestamp();
        if (IsHandleCreated && CanAcceptMovement()) movementTimer.Start();
        else movementTimer.Stop();
    }

    void BindOwnerForm()
    {
        Form next = FindForm();
        if (next == ownerForm) return;
        UnbindOwnerForm();
        ownerForm = next;
        if (ownerForm == null) return;
        ownerForm.Deactivate += OwnerDeactivated;
        ownerForm.Activated += OwnerActivated;
        ownerForm.EnabledChanged += OwnerEnabledChanged;
    }

    void UnbindOwnerForm()
    {
        if (ownerForm == null) return;
        ownerForm.Deactivate -= OwnerDeactivated;
        ownerForm.Activated -= OwnerActivated;
        ownerForm.EnabledChanged -= OwnerEnabledChanged;
        ownerForm = null;
    }

    void OwnerDeactivated(object sender, EventArgs e)
    {
        ClearInput();
        movementTimer.Stop();
    }
    void OwnerEnabledChanged(object sender, EventArgs e)
    {
        if (ownerForm != null && !ownerForm.Enabled) ClearInput();
        RefreshTimer();
    }
    void OwnerActivated(object sender, EventArgs e)
    {
        if (refocusAfterQuest && !actionInProgress && Visible && Enabled) {
            refocusAfterQuest = false;
            Focus();
        }
        lastTick = Stopwatch.GetTimestamp();
        RefreshTimer();
    }

    void ClearInput()
    {
        if (clearingInput) return;
        clearingInput = true;
        try {
            bool changed = walking || movementKeys.Count > 0 || keyboardHoldDirection != 0 || pressedButton >= 0 || Capture;
            walking = false;
            walkTime = 0;
            movementKeys.Clear();
            keyboardHoldDirection = 0;
            pressedButton = -1;
            if (Capture) Capture = false;
            lastTick = Stopwatch.GetTimestamp();
            if (!disposed && changed) Invalidate();
        }
        finally { clearingInput = false; }
    }

    void LayoutButtons()
    {
        int margin = Math.Min(18, Math.Max(4, Width / 30));
        int topWidth = Math.Min(190, Math.Max(1, (Width - margin * 3) / 2));
        int height = Math.Min(56, Math.Max(24, Height / 10));
        buttonBounds[ReturnButton] = new Rectangle(margin, margin, topWidth, height);
        int questWidth = Math.Min(170, topWidth);
        buttonBounds[QuestButton] = new Rectangle(Math.Max(margin, Width - margin - questWidth), margin, questWidth, height);
        int movementWidth = Math.Min(72, Math.Max(28, (Width - margin * 3) / 3));
        int bottomY = Math.Max(margin, Height - margin - height);
        buttonBounds[LeftButton] = new Rectangle(Math.Max(margin, Width - margin * 2 - movementWidth * 2), bottomY, movementWidth, height);
        buttonBounds[RightButton] = new Rectangle(Math.Max(margin, Width - margin - movementWidth), bottomY, movementWidth, height);
        buttonBounds[MillButton] = new Rectangle(Math.Max(margin,(Width-200)/2),margin+height+50,200,44);
    }

    int HitButton(Point point)
    {
        LayoutButtons();
        for (int i = 0; i < buttonBounds.Length; i++) if ((i!=MillButton||OpenMill!=null)&&buttonBounds[i].Contains(point)) return i;
        return -1;
    }

    string CurrentArea()
    {
        string area = "村口";
        for (int i = 0; i < AreaStarts.Length && playerX >= AreaStarts[i]; i++) area = AreaNames[i];
        return area;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        LayoutButtons();
        Graphics g = e.Graphics;
        g.Clear(BackColor);
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.SmoothingMode = SmoothingMode.None;
        float scale = MapScale;
        g.DrawImage(panorama, new RectangleF(MapOffsetX - cameraX * scale, 0,
            panorama.Width * scale, panorama.Height * scale),
            new RectangleF(0, 0, panorama.Width, panorama.Height), GraphicsUnit.Pixel);

        float x = MapOffsetX + (playerX - cameraX) * scale;
        float feetY = GroundYAt(playerX) * scale;
        var state = g.Save();
        g.TranslateTransform(x, feetY);
        g.ScaleTransform(facingLeft ? -scale : scale, scale);
        if (walking && walkFrames != null) {
            // Shared canvas anchor: no per-frame bounding-box centering or rescaling.
            Image frame = walkFrames[WalkFrameIndex];
            g.DrawImage(frame, new RectangleF(-70, -146, 136, 152),
                new RectangleF(0, 0, 136, 152), GraphicsUnit.Pixel);
        } else {
            g.DrawImage(hero, new RectangleF(-hero.Width * 0.5f, -hero.Height, hero.Width, hero.Height),
                new RectangleF(0, 0, hero.Width, hero.Height), GraphicsUnit.Pixel);
        }
        g.Restore(state);

        string[] captions = { "返回大地图", "主线任务", "←", "→" };
        for (int i = 0; i < buttonBounds.Length; i++) {
            if(i==MillButton)continue;
            bool active = hoveredButton == i || pressedButton == i || (Focused && keyboardButton == i);
            GuildChrome.Draw(g, buttonBounds[i], active, Enabled);
            using (Font font = GameTheme.Body(i < 2 ? 13 : 21, FontStyle.Bold))
                GameTheme.DrawText(g, captions[i], font, buttonBounds[i], GuildChrome.Ivory,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        int titleWidth = Math.Min(360, Math.Max(1, Width - 36));
        int freeWidth = buttonBounds[QuestButton].Left - buttonBounds[ReturnButton].Right - 24;
        int titleY = freeWidth >= 180 ? 18 : buttonBounds[ReturnButton].Bottom + 8;
        if (freeWidth >= 180) titleWidth = Math.Min(titleWidth, freeWidth);
        var title = new Rectangle((Width - titleWidth) / 2, titleY, titleWidth, 42);
        GuildChrome.Draw(g, title);
        using (Font font = GameTheme.Body(12, FontStyle.Bold))
            GameTheme.DrawText(g, "麦溪村 · " + CurrentArea(), font, title, GuildChrome.Ivory,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        if(OpenMill!=null){GuildChrome.Draw(g,buttonBounds[MillButton],hoveredButton==MillButton);using(var font=GameTheme.Body(12,FontStyle.Bold))GameTheme.DrawText(g,"磨坊地牢试玩 · E",font,buttonBounds[MillButton],GuildChrome.Ivory,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);}

        int hintWidth = Math.Max(1, buttonBounds[LeftButton].Left - 24);
        var hint = new Rectangle(14, buttonBounds[LeftButton].Top, hintWidth, buttonBounds[LeftButton].Height);
        using (var shade = new SolidBrush(Color.FromArgb(165, 17, 37, 31))) g.FillRectangle(shade, hint);
        using (Font font = GameTheme.Body(11))
            GameTheme.DrawText(g, "A / D · ← / → 移动", font, hint, GuildChrome.Ivory,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    void ActivateButton(int button)
    {
        if (button < 0 || (button > QuestButton && button!=MillButton) || disposed || !Enabled) return;
        ClearInput();
        actionInProgress = true;
        movementTimer.Stop();
        if (button == QuestButton) refocusAfterQuest = true;
        try {
            Action action = button == ReturnButton ? Return : button==MillButton?OpenMill:OpenQuest;
            if (action != null) action();
        }
        finally {
            actionInProgress = false;
            if (!disposed) {
                ClearInput();
                if (button == QuestButton && Visible && Enabled &&
                    (ownerForm == null || Form.ActiveForm == ownerForm)) {
                    refocusAfterQuest = false;
                    Focus();
                }
                RefreshTimer();
            }
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left || !Enabled || actionInProgress) return;
        ClearInput();
        Focus();
        keyboardButton = -1;
        pressedButton = HitButton(e.Location);
        if (pressedButton >= 0) Capture = true;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left) return;
        int pressed = pressedButton;
        int released = HitButton(e.Location);
        ClearInput();
        if (pressed == released) ActivateButton(pressed);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        int hovered = HitButton(e.Location);
        if (hovered == hoveredButton) return;
        hoveredButton = hovered;
        Cursor = hovered >= 0 ? Cursors.Hand : Cursors.Default;
        Invalidate();
    }
    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        hoveredButton = -1;
        Invalidate();
    }
    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);
        if (!Capture) ClearInput();
    }

    protected override bool IsInputKey(Keys keyData)
    {
        Keys key = keyData & Keys.KeyCode;
        if (key == Keys.Left || key == Keys.Right || key == Keys.A || key == Keys.D ||
            ((key == Keys.Enter || key == Keys.Space) && keyboardButton >= 0)) return true;
        return base.IsInputKey(keyData);
    }

    protected override bool ProcessDialogKey(Keys keyData)
    {
        // Escape deliberately falls through to the main form's existing settings action.
        if ((keyData & Keys.KeyCode) == Keys.Tab && (keyData & (Keys.Control | Keys.Alt)) == 0) {
            ClearInput();
            bool backward = (keyData & Keys.Shift) != 0;
            int count=OpenMill==null?4:5;
            keyboardButton = keyboardButton < 0 ? (backward ? count-1 : 0) : (keyboardButton + (backward ? count-1 : 1)) % count;
            Invalidate();
            return true;
        }
        return base.ProcessDialogKey(keyData);
    }

    protected override bool ProcessKeyEventArgs(ref Message message)
    {
        bool previous = currentKeyIsRepeat;
        currentKeyIsRepeat = (message.Msg == 0x100 || message.Msg == 0x104) &&
            (message.LParam.ToInt64() & (1L << 30)) != 0;
        try { return base.ProcessKeyEventArgs(ref message); }
        finally { currentKeyIsRepeat = previous; }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!CanAcceptMovement() || e.Control || e.Alt) return;
        if(e.KeyCode==Keys.E&&OpenMill!=null){e.Handled=e.SuppressKeyPress=true;if(!currentKeyIsRepeat)ActivateButton(MillButton);return;}
        if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.A || e.KeyCode == Keys.D) {
            // A held key cannot restart walking when focus returns from a dialog.
            if (!currentKeyIsRepeat || movementKeys.Contains(e.KeyCode)) movementKeys.Add(e.KeyCode);
            e.Handled = e.SuppressKeyPress = true;
        }
        else if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) && keyboardButton >= 0) {
            e.Handled = e.SuppressKeyPress = true;
            if (currentKeyIsRepeat) return;
            if (keyboardButton <= QuestButton||keyboardButton==MillButton) ActivateButton(keyboardButton);
            else keyboardHoldDirection = keyboardButton == LeftButton ? -1 : 1;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (movementKeys.Remove(e.KeyCode)) e.Handled = true;
        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) keyboardHoldDirection = 0;
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        lastTick = Stopwatch.GetTimestamp();
        RefreshTimer();
        Invalidate();
    }
    protected override void OnLostFocus(EventArgs e)
    {
        ClearInput();
        if (movementTimer != null) movementTimer.Stop();
        base.OnLostFocus(e);
        Invalidate();
    }
    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        BindOwnerForm();
        ClearInput();
        RefreshTimer();
    }
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        BindOwnerForm();
        cameraX = Clamp(playerX - WorldViewportWidth * 0.45f, 0f, MaximumCameraX);
        RefreshTimer();
    }
    protected override void OnHandleDestroyed(EventArgs e)
    {
        ClearInput();
        if (movementTimer != null) movementTimer.Stop();
        UnbindOwnerForm();
        base.OnHandleDestroyed(e);
    }
    protected override void OnVisibleChanged(EventArgs e)
    {
        if (!Visible) ClearInput();
        base.OnVisibleChanged(e);
        RefreshTimer();
    }
    protected override void OnEnabledChanged(EventArgs e)
    {
        if (!Enabled) ClearInput();
        base.OnEnabledChanged(e);
        RefreshTimer();
    }
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (panorama == null || hero == null) return;
        playerX = Clamp(playerX, MinimumPlayerX, MaximumPlayerX);
        cameraX = Clamp(playerX - WorldViewportWidth * 0.45f, 0f, MaximumCameraX);
        LayoutButtons();
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !disposed) {
            ClearInput();
            disposed = true;
            movementTimer.Stop();
            movementTimer.Tick -= MovementTimerTick;
            movementTimer.Dispose();
            if (ownsWalkFrames) foreach (Image frame in walkFrames) frame.Dispose();
            UnbindOwnerForm();
            Return = null;
            OpenQuest = null;
            OpenMill = null;
        }
        base.Dispose(disposing);
    }
}


