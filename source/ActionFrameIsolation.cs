using System;using System.Collections.Generic;using System.Drawing;using System.Drawing.Imaging;
public partial class PartyBattleCanvas {
 readonly Dictionary<string,Bitmap> isolatedActions=new Dictionary<string,Bitmap>();
 // Extract each sprite into its own bitmap: neighbouring atlas pixels can never
 // be sampled, and disconnected pieces entering from another cell are discarded.
 Bitmap ActionFrame(Image atlas,Rectangle cell,string key,bool isolate){
  Bitmap cached;if(isolatedActions.TryGetValue(key,out cached))return cached;
  var result=((Bitmap)atlas).Clone(cell,PixelFormat.Format32bppArgb);
  if(isolate){int w=result.Width,h=result.Height;var data=result.LockBits(new Rectangle(0,0,w,h),ImageLockMode.ReadWrite,PixelFormat.Format32bppArgb);var bytes=new byte[data.Stride*h];System.Runtime.InteropServices.Marshal.Copy(data.Scan0,bytes,0,bytes.Length);
   var labels=new int[w*h];int label=0,best=0,bestCount=0;var queue=new Queue<int>();
   for(int start=0;start<labels.Length;start++){if(labels[start]!=0||bytes[(start/w)*data.Stride+(start%w)*4+3]<64)continue;label++;labels[start]=label;queue.Enqueue(start);int count=0;while(queue.Count>0){int at=queue.Dequeue(),x=at%w,y=at/w;count++;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int xx=x+dx,yy=y+dy;if(xx<0||xx>=w||yy<0||yy>=h)continue;int n=yy*w+xx;if(labels[n]==0&&bytes[yy*data.Stride+xx*4+3]>=64){labels[n]=label;queue.Enqueue(n);}}}if(count>bestCount){bestCount=count;best=label;}}
   for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(labels[y*w+x]!=best)bytes[y*data.Stride+x*4+3]=0;
   System.Runtime.InteropServices.Marshal.Copy(bytes,0,data.Scan0,bytes.Length);result.UnlockBits(data);
  }isolatedActions[key]=result;return result;
 }
 void DrawIsolatedAction(Graphics g,Image atlas,Rectangle cell,Rectangle destination,string key,bool isolate,float opacity=1){
  if(!isolate){var directFrame=ActionFrame(atlas,cell,key,false);using(var attr=new ImageAttributes()){var matrix=new ColorMatrix();matrix.Matrix33=opacity;attr.SetColorMatrix(matrix);g.DrawImage(directFrame,destination,0,0,directFrame.Width,directFrame.Height,GraphicsUnit.Pixel,attr);}return;}
  int px=cell.Width/6,py=key.StartsWith("monster:")?0:cell.Height/10;
  var area=Rectangle.Intersect(new Rectangle(cell.X-px,cell.Y-py,cell.Width+px*2,cell.Height+py*2),new Rectangle(0,0,atlas.Width,atlas.Height));
  var sprite=ActionFrame(atlas,area,key,true);
  float sx=destination.Width/(float)cell.Width,sy=destination.Height/(float)cell.Height;
  var expanded=new RectangleF(destination.X+(area.X-cell.X)*sx,destination.Y+(area.Y-cell.Y)*sy,area.Width*sx,area.Height*sy);
  using(var attr=new ImageAttributes()){var matrix=new ColorMatrix();matrix.Matrix33=opacity;attr.SetColorMatrix(matrix);g.DrawImage(sprite,Rectangle.Round(expanded),0,0,sprite.Width,sprite.Height,GraphicsUnit.Pixel,attr);}
 }
 void DisposeActionFrames(){if(femalePaletteActions!=null){femalePaletteActions.Dispose();femalePaletteActions=null;femalePaletteSource=null;}foreach(var frame in isolatedActions.Values)frame.Dispose();isolatedActions.Clear();}
}

