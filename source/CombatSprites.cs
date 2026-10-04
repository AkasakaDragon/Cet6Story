using System;using System.Drawing;using System.Drawing.Imaging;using System.Collections.Generic;

// Decode generated sprite atlases as separate connected characters. A regular
// grid alone can include disconnected fragments from the neighbouring pose.
// Optional lower-row overlap preserves support boots that extend below the atlas midpoint.
public static class CombatSprites {
 public static Bitmap Character(Image sheet,int cell,int bottomPadding=0){int w=sheet.Width/4,rowHeight=sheet.Height/2,sourceY=cell/4*rowHeight,h=Math.Min(rowHeight+bottomPadding,sheet.Height-sourceY);using(var raw=new Bitmap(w,h,PixelFormat.Format32bppArgb)){
  using(var g=Graphics.FromImage(raw))g.DrawImage(sheet,new Rectangle(0,0,w,h),new Rectangle(cell%4*w,sourceY,w,h),GraphicsUnit.Pixel);
  var visited=new bool[w*h];var best=new List<int>();var queue=new Queue<int>();
  for(int y=0;y<h;y++)for(int x=0;x<w;x++){int start=y*w+x;if(visited[start]||raw.GetPixel(x,y).A<64)continue;var component=new List<int>();queue.Enqueue(start);visited[start]=true;while(queue.Count>0){int p=queue.Dequeue();component.Add(p);int px=p%w,py=p/w;foreach(int n in new[]{px>0?p-1:-1,px<w-1?p+1:-1,py>0?p-w:-1,py<h-1?p+w:-1})if(n>=0&&!visited[n]){visited[n]=true;if(raw.GetPixel(n%w,n/w).A>=64)queue.Enqueue(n);}}if(component.Count>best.Count)best=component;}
  var result=new Bitmap(w,h,PixelFormat.Format32bppArgb);if(best.Count==0)return result;int dy=0;
  foreach(int p in best){int x=p%w,y=p/w+dy;if(y>=0&&y<h)result.SetPixel(x,y,raw.GetPixel(p%w,p/w));}int left=w,top=h,right=-1,last=-1;for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(result.GetPixel(x,y).A>=64){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);last=Math.Max(last,y);}if(right>=left){var cropped=result.Clone(Rectangle.FromLTRB(left,top,right+1,last+1),PixelFormat.Format32bppArgb);result.Dispose();return cropped;}return result;
 }}
 public static Bitmap Effect(Image sheet,int cell){int w=sheet.Width/4,h=sheet.Height/2;var result=new Bitmap(w,h,PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(result))g.DrawImage(sheet,new Rectangle(0,0,w,h),new Rectangle(cell%4*w,cell/4*h,w,h),GraphicsUnit.Pixel);return result;}
}

