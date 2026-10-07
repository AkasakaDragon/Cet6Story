using System.Drawing;
public static class MonsterAnimationArt {
 public static Rectangle Cell(Image sheet,int row,int frame){int[] edges={0,230,477,686,910,1145};int top=edges[row]*sheet.Height/1145,bottom=edges[row+1]*sheet.Height/1145,left=frame*sheet.Width/6,right=(frame+1)*sheet.Width/6;return new Rectangle(left,top,right-left,bottom-top);}
}
