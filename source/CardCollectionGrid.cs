using System.Windows.Forms;

// Opaque, buffered card backgrounds prevent native scroll blits from leaving old
// transparent card pixels behind when scrolling the nested collection surface.
public sealed class CardCollectionGrid:FlowLayoutPanel {
 public CardCollectionGrid(){DoubleBuffered=true;ResizeRedraw=true;BackColor=GameTheme.Navy;SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);}
}
