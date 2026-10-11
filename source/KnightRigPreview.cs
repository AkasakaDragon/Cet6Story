using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;

public sealed class KnightRigPreview:PixelFrame {
 readonly PixelSkeletalRig rig;
 readonly KnightRigStage stage;
 readonly Timer timer=new Timer{Interval=33};
 readonly Stopwatch clock=Stopwatch.StartNew();
 readonly Label boneLabel,status;
 string group="body";
 int selected;
 bool paused;
 float previous,windTime,clipTime,speed=1;
 readonly string projectRoot;
 public KnightRigPreview(string root) {
  projectRoot=root;
  rig=new PixelSkeletalRig(Path.Combine(root,"assets","characters","red-knight-rig"));
  string savedPose=Path.Combine(root,"output","knight-rig","pose-offsets.json");
  if(File.Exists(savedPose)) {
   var saved=new JavaScriptSerializer().Deserialize<Dictionary<string,float>>(File.ReadAllText(savedPose));
   foreach(var pair in saved)if(rig.Definition.bones.Any(b=>b.name==pair.Key))rig.AngleOffsets[pair.Key]=pair.Value;
  }
  Text="女骑士 · 骨骼动画工作台";ShowInTaskbar=true;StartPosition=FormStartPosition.CenterScreen;
  Size=new Size(1040,780);MinimumSize=new Size(740,640);KeyPreview=true;
  var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,BackColor=BackColor};
  layout.RowStyles.Add(new RowStyle(SizeType.Absolute,48));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,112));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,32));Controls.Add(layout);
  var actions=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=6,RowCount=1,BackColor=BackColor};
  for(int i=0;i<6;i++)actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f/6));
  AddButton(actions,"待机呼吸",0,()=>Play("idle"));AddButton(actions,"双手挥剑",1,()=>Play("attack"));AddButton(actions,"受击后仰",2,()=>Play("hit"));
  GameButton pause=null;pause=AddButton(actions,"暂停 / 继续",3,()=>{paused=!paused;pause.Primary=paused;pause.Invalidate();});
  AddButton(actions,"速度 ×1 / ×0.5",4,()=>{speed=speed==1?.5f:1;});AddButton(actions,"重置姿势",5,()=>{rig.Reset();selected=0;clipTime=0;UpdateSelection();});layout.Controls.Add(actions,0,0);
  stage=new KnightRigStage(rig){Dock=DockStyle.Fill,BackColor=Color.FromArgb(17,40,38)};layout.Controls.Add(stage,0,1);
  var tools=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=6,RowCount=3,BackColor=BackColor};
  for(int i=0;i<6;i++)tools.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f/6));
  var skeleton=Toggle("显示骨架",true,v=>stage.ShowBones=v);tools.Controls.Add(skeleton,0,0);
  tools.Controls.Add(Toggle("发梢 / 披风摆动",true,v=>rig.Wind=v),1,0);tools.SetColumnSpan(tools.GetControlFromPosition(1,0),2);
  tools.Controls.Add(Toggle("叠加原八帧",true,v=>rig.HybridWind=v),3,0);
  tools.Controls.Add(Toggle("双手贴合剑柄",true,v=>rig.UseArmIK=v),4,0);tools.SetColumnSpan(tools.GetControlFromPosition(4,0),2);
  AddButton(tools,"身体关节",0,()=>SelectGroup("body"),1);AddButton(tools,"头发骨链",1,()=>SelectGroup("hair"),1);AddButton(tools,"披风骨链",2,()=>SelectGroup("cape"),1);
  AddButton(tools,"前一个关节",3,()=>{selected--;UpdateSelection();},1);AddButton(tools,"后一个关节",4,()=>{selected++;UpdateSelection();},1);
  AddButton(tools,"保存调节",5,SaveOffsets,1);
  boneLabel=new Label{Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,ForeColor=GameTheme.Gold};tools.Controls.Add(boneLabel,0,2);tools.SetColumnSpan(boneLabel,3);
  AddButton(tools,"角度 −5°",3,()=>Adjust(-5),2);AddButton(tools,"角度 +5°",4,()=>Adjust(5),2);AddButton(tools,"逐步播放",5,()=>{paused=true;Advance(.12f);},2);
  layout.Controls.Add(tools,0,2);
  status=new Label{Dock=DockStyle.Fill,ForeColor=GameTheme.Muted,TextAlign=ContentAlignment.MiddleLeft};layout.Controls.Add(status,0,3);
  UpdateSelection();
  timer.Tick+=(s,e)=>{float now=(float)clock.Elapsed.TotalSeconds,dt=Math.Min(.1f,Math.Max(0,now-previous));previous=now;if(!paused)Advance(dt*speed);else stage.Invalidate();};
  Shown+=(s,e)=>{previous=(float)clock.Elapsed.TotalSeconds;timer.Start();};
  KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Space){paused=!paused;e.Handled=true;}if(e.KeyCode==Keys.Escape)Close();};
 }
 GameButton AddButton(TableLayoutPanel parent,string text,int column,Action action,int row=0) {
  var b=new GameButton{Text=text,Dock=DockStyle.Fill,Margin=new Padding(3),BackColor=BackColor,ForeColor=GameTheme.Ink};b.Click+=(s,e)=>action();parent.Controls.Add(b,column,row);return b;
 }
 GoldCheckBox Toggle(string text,bool value,Action<bool> change) {
  var c=new GoldCheckBox{Text=text,Checked=value,Dock=DockStyle.Fill,ForeColor=GameTheme.Ink,Font=GameTheme.Body(10)};c.CheckedChanged+=(s,e)=>{change(c.Checked);stage.Invalidate();};return c;
 }
 void Play(string name){rig.Animation=name;clipTime=0;rig.AngleOffsets.Clear();paused=false;stage.Invalidate();}
 void Advance(float dt) {
  windTime+=dt;clipTime+=dt;var clip=rig.Definition.clips.First(c=>c.name==rig.Animation);
  if(!clip.loop && clipTime>=clip.duration){rig.Animation="idle";clipTime=0;}
  rig.AnimationTime=clipTime;rig.WindTime=windTime;status.Text=rig.Definition.bones.Length+" 个骨骼 · "+rig.Definition.layers.Length+" 个图层 · "+rig.Definition.clips.First(c=>c.name==rig.Animation).label+" · 原图拆层原型";stage.Invalidate();
 }
 PixelRigBone[] Group(){return rig.Definition.bones.Where(b=>b.group==group).ToArray();}
 void SelectGroup(string value){group=value;selected=0;UpdateSelection();}
 void UpdateSelection() {
  var bones=Group();selected=(selected%bones.Length+bones.Length)%bones.Length;var bone=bones[selected];
  float angle;rig.AngleOffsets.TryGetValue(bone.name,out angle);boneLabel.Text=bone.label+"  ("+(selected+1)+" / "+bones.Length+")  · "+angle.ToString("0")+"°";
  if(stage!=null){stage.SelectedBone=bone.name;stage.Invalidate();}
 }
 void Adjust(float value) {
  paused=true;var name=Group()[selected].name;float angle;rig.AngleOffsets.TryGetValue(name,out angle);rig.AngleOffsets[name]=Math.Max(-160,Math.Min(160,angle+value));UpdateSelection();
 }
 void SaveOffsets() {
  string folder=Path.Combine(projectRoot,"output","knight-rig");Directory.CreateDirectory(folder);
  File.WriteAllText(Path.Combine(folder,"pose-offsets.json"),new JavaScriptSerializer().Serialize(rig.AngleOffsets));status.Text="关节调节已保存到 output / knight-rig / pose-offsets.json";
 }
 protected override void Dispose(bool disposing){if(disposing){timer.Stop();timer.Dispose();}base.Dispose(disposing);}
}

public sealed class KnightRigStage:Control {
 readonly PixelSkeletalRig rig;
 public bool ShowBones=true;
 public string SelectedBone="root";
 public KnightRigStage(PixelSkeletalRig rig){this.rig=rig;DoubleBuffered=true;SetStyle(ControlStyles.ResizeRedraw,true);}
 protected override void OnPaint(PaintEventArgs e) {
  base.OnPaint(e);var g=e.Graphics;g.Clear(BackColor);g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
  const int width=400,height=340;const float ox=55,oy=36;
  float scale=Math.Min(Width/(float)width,Height/(float)height);
  float x=(Width-width*scale)/2,y=(Height-height*scale)/2;
  using(var grid=new Pen(Color.FromArgb(28,GameTheme.Gold)))for(int gx=0;gx<width;gx+=20)g.DrawLine(grid,x+gx*scale,y,x+gx*scale,y+height*scale);
  using(var ground=new Pen(Color.FromArgb(90,GameTheme.Gold)))g.DrawLine(ground,0,y+(oy+269)*scale,Width,y+(oy+269)*scale);
  using(var image=rig.Render(width,height,ox,oy))g.DrawImage(image,new RectangleF(x,y,width*scale,height*scale));
  if(ShowBones)for(int i=0;i<rig.Definition.bones.Length;i++) {
   var bone=rig.Definition.bones[i];var joint=rig.Joint(bone.name);
   Color color=bone.name==SelectedBone?Color.White:bone.group=="hair"?Color.FromArgb(255,105,157):bone.group=="cape"?Color.FromArgb(255,184,61):Color.FromArgb(80,210,245);
   float bx=x+(joint.X+ox)*scale,by=y+(joint.Y+oy)*scale;
   using(var pen=new Pen(color,1.5f))if(!String.IsNullOrEmpty(bone.parent)){var parent=rig.Joint(bone.parent);g.DrawLine(pen,x+(parent.X+ox)*scale,y+(parent.Y+oy)*scale,bx,by);}
   using(var fill=new SolidBrush(color))g.FillRectangle(fill,bx-2,by-2,5,5);
  }
 }
}
