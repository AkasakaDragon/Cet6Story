using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;

// Data-driven cutout rig. Coordinates are logical pixels, angles are degrees.
// Textures stay untouched; nearest-neighbour triangle sampling preserves pixels.
public sealed class PixelRigBone {
 public string name, parent, label, group;
 public float x, y;
}
public sealed class PixelRigLayer {
 public string name, bone, image, skin;
 public string[] variants;
 public int z;
}
public sealed class PixelRigKey {
 public float t, rootX, chestY, chestAngle, headAngle, weaponX, weaponY, weaponAngle;
}
public sealed class PixelRigClip {
 public string name, label;
 public float duration;
 public bool loop;
 public PixelRigKey[] keys;
}
public sealed class PixelRigDefinition {
 public int version, width, height;
 public float frameSeconds;
 public PixelRigBone[] bones;
 public PixelRigLayer[] layers;
 public PixelRigClip[] clips;
}
public sealed class PixelRigJoint {
 public float X, Y, Angle;
 internal float LocalAngle, Dx, Dy;
}

public sealed class PixelSkeletalRig {
 sealed class Vertex {
  public float X,Y;
  public int[] Bones;
  public float[] Weights;
 }
 sealed class Texture {
  public PixelRigLayer Definition;
  public int Bone;
  public int[][] Pixels;
  public Vertex[] Vertices;
  public int[] Indices;
 }
 public readonly PixelRigDefinition Definition;
 readonly Dictionary<string,int> indices=new Dictionary<string,int>();
 readonly PixelRigJoint[] joints;
 readonly Texture[] textures;
 readonly int[] parents;
 public readonly Dictionary<string,float> AngleOffsets=new Dictionary<string,float>();
 public bool UseArmIK=true;
 public bool Wind=true;
 public bool HybridWind=true;
 public string Animation="idle";
 public float AnimationTime;
 public float WindTime;
 const float Rad=(float)(Math.PI/180);

 public PixelSkeletalRig(string folder) {
  Definition=new JavaScriptSerializer().Deserialize<PixelRigDefinition>(File.ReadAllText(Path.Combine(folder,"rig.json")));
  if(Definition.version!=1 || Definition.width<1 || Definition.height<1)throw new InvalidDataException("不支持的角色骨骼数据。");
  joints=new PixelRigJoint[Definition.bones.Length];parents=new int[joints.Length];
  for(int i=0;i<joints.Length;i++) {
   var b=Definition.bones[i];
   if(indices.ContainsKey(b.name))throw new InvalidDataException("重复骨骼："+b.name);
   int p=-1;
   if(!String.IsNullOrEmpty(b.parent) && !indices.TryGetValue(b.parent,out p))throw new InvalidDataException("父骨骼必须先定义："+b.name);
   parents[i]=p;indices.Add(b.name,i);joints[i]=new PixelRigJoint();
  }
  textures=Definition.layers.OrderBy(l=>l.z).Select(l=>LoadTexture(folder,l)).ToArray();
  Evaluate();
 }
 static int[] ReadPixels(string path,int width,int height) {
  using(var original=new Bitmap(path)) {
   if(original.Width!=width || original.Height!=height)throw new InvalidDataException("分层图片尺寸不一致："+path);
   {
    var locked=original.LockBits(new Rectangle(0,0,width,height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
    try {var pixels=new int[width*height];for(int y=0;y<height;y++)Marshal.Copy(IntPtr.Add(locked.Scan0,y*locked.Stride),pixels,y*width,width);return pixels;}
    finally {original.UnlockBits(locked);}
   }
  }
 }
 Texture LoadTexture(string folder,PixelRigLayer layer) {
  int bone;if(!indices.TryGetValue(layer.bone,out bone))throw new InvalidDataException("无效分层骨骼："+layer.name);
  var files=new List<string>{layer.image};if(layer.variants!=null)files.AddRange(layer.variants);
  var t=new Texture{Definition=layer,Bone=bone,Pixels=files.Select(f=>ReadPixels(Path.Combine(folder,f),Definition.width,Definition.height)).ToArray()};
  int x0=Definition.width,y0=Definition.height,x1=0,y1=0;
  foreach(var pixels in t.Pixels)for(int y=0;y<Definition.height;y++)for(int x=0;x<Definition.width;x++)if((uint)pixels[y*Definition.width+x]>>24!=0){x0=Math.Min(x0,x);y0=Math.Min(y0,y);x1=Math.Max(x1,x+1);y1=Math.Max(y1,y+1);}
  if(x1<=x0){t.Vertices=new Vertex[0];t.Indices=new int[0];return t;}
  bool cloth=!String.IsNullOrEmpty(layer.skin);
  int nx=cloth?(int)Math.Ceiling((x1-x0)/12.0):1,ny=cloth?(int)Math.Ceiling((y1-y0)/12.0):1;
  var vertices=new List<Vertex>();var triangles=new List<int>();
  for(int y=0;y<=ny;y++)for(int x=0;x<=nx;x++) {
   var v=new Vertex{X=x0+(x1-x0)*x/(float)nx,Y=y0+(y1-y0)*y/(float)ny};
   if(!cloth){v.Bones=new[]{bone};v.Weights=new[]{1f};}
   else {
    // Three nearest control nodes form a normalized skinning weight map.
    var nearest=Enumerable.Range(0,joints.Length).Where(i=>Definition.bones[i].group==layer.skin)
     .OrderBy(i=>DistanceSquared(v.X,v.Y,Definition.bones[i].x,Definition.bones[i].y)).Take(3).ToArray();
    v.Bones=nearest;v.Weights=nearest.Select(i=>1f/(12+DistanceSquared(v.X,v.Y,Definition.bones[i].x,Definition.bones[i].y))).ToArray();
    float sum=v.Weights.Sum();for(int i=0;i<v.Weights.Length;i++)v.Weights[i]/=sum;
   }
   vertices.Add(v);
  }
  for(int y=0;y<ny;y++)for(int x=0;x<nx;x++){int a=y*(nx+1)+x,b=a+1,c=a+nx+1,d=c+1;triangles.AddRange(new[]{a,b,c,b,d,c});}
  t.Vertices=vertices.ToArray();t.Indices=triangles.ToArray();return t;
 }
 static float DistanceSquared(float x,float y,float a,float b){return (x-a)*(x-a)+(y-b)*(y-b);}
 public PixelRigJoint Joint(string name){return joints[indices[name]];}
 public void Reset(){AngleOffsets.Clear();Animation="idle";AnimationTime=0;}
 public void Evaluate() {
  foreach(var j in joints){j.LocalAngle=0;j.Dx=0;j.Dy=0;}
  var clip=Definition.clips.FirstOrDefault(c=>c.name==Animation);
  if(clip==null)throw new ArgumentException("未知动画："+Animation);
  float t=clip.loop?AnimationTime%clip.duration:Math.Min(AnimationTime,clip.duration);
  var key=Sample(clip,t);
  Joint("root").Dx=key.rootX;Joint("chest").Dy=key.chestY;Joint("chest").LocalAngle=key.chestAngle;
  Joint("head").LocalAngle=key.headAngle;Joint("weapon").Dx=key.weaponX;Joint("weapon").Dy=key.weaponY;Joint("weapon").LocalAngle=key.weaponAngle;
  if(Wind)for(int i=0;i<joints.Length;i++) {
   var b=Definition.bones[i];if(b.group=="body")continue;
   int node=b.name[b.name.Length-1]-'0';
   double phase=WindTime*Math.PI*2/1.92-node*.55+(b.name.Contains("top")?.4:b.name.Contains("low")?-.5:b.name.Contains("b")?.7:0);
   // Roots are restrained; tips lag and swing more. Recoil carries into cloth.
   joints[i].LocalAngle=(float)Math.Sin(phase)*(b.group=="hair"?1.6f:1.1f)*(node+1)+key.chestAngle*node*-.12f;
  }
  UpdateWorld();
  if(UseArmIK){ConstrainGrip();SolveArm("near_upper","near_lower","near_hand");SolveArm("far_upper","far_lower","far_hand");}
  foreach(var offset in AngleOffsets){int i;if(indices.TryGetValue(offset.Key,out i))joints[i].LocalAngle+=offset.Value;}
  UpdateWorld();
 }
 static PixelRigKey Sample(PixelRigClip clip,float t) {
  var a=clip.keys[0];var b=a;
  for(int i=1;i<clip.keys.Length;i++){b=clip.keys[i];if(t<=b.t)break;a=b;}
  float u=b.t<=a.t?0:Math.Max(0,Math.Min(1,(t-a.t)/(b.t-a.t)));u=u*u*(3-2*u);
  return new PixelRigKey{rootX=Lerp(a.rootX,b.rootX,u),chestY=Lerp(a.chestY,b.chestY,u),chestAngle=Lerp(a.chestAngle,b.chestAngle,u),headAngle=Lerp(a.headAngle,b.headAngle,u),weaponX=Lerp(a.weaponX,b.weaponX,u),weaponY=Lerp(a.weaponY,b.weaponY,u),weaponAngle=Lerp(a.weaponAngle,b.weaponAngle,u)};
 }
 static float Lerp(float a,float b,float t){return a+(b-a)*t;}
 void UpdateWorld() {
  for(int i=0;i<joints.Length;i++) {
   var b=Definition.bones[i];var j=joints[i];int p=parents[i];
   if(p<0){j.X=b.x+j.Dx;j.Y=b.y+j.Dy;j.Angle=j.LocalAngle;continue;}
   var parent=joints[p];var rest=Definition.bones[p];
   float dx=b.x-rest.x+j.Dx,dy=b.y-rest.y+j.Dy,c=(float)Math.Cos(parent.Angle*Rad),s=(float)Math.Sin(parent.Angle*Rad);
   j.X=parent.X+dx*c-dy*s;j.Y=parent.Y+dx*s+dy*c;j.Angle=parent.Angle+j.LocalAngle;
  }
 }
 PointF Transform(int i,float x,float y) {
  var b=Definition.bones[i];var j=joints[i];float c=(float)Math.Cos(j.Angle*Rad),s=(float)Math.Sin(j.Angle*Rad);
  return new PointF(j.X+(x-b.x)*c-(y-b.y)*s,j.Y+(x-b.x)*s+(y-b.y)*c);
 }
 PointF Grip(string hand){var b=Definition.bones[indices[hand]];return Transform(indices["weapon"],b.x,b.y);}
 void ConstrainGrip() {
  // Project the shared weapon target into both arms' reachable circles.
  // This prevents a two-handed attack from tearing either hand off the hilt.
  for(int k=0;k<12;k++)foreach(string side in new[]{"near","far"}) {
   int u=indices[side+"_upper"],l=indices[side+"_lower"],h=indices[side+"_hand"];
   var upper=Definition.bones[u];var lower=Definition.bones[l];var hand=Definition.bones[h];
   float l1=(float)Math.Sqrt(DistanceSquared(upper.x,upper.y,lower.x,lower.y)),l2=(float)Math.Sqrt(DistanceSquared(lower.x,lower.y,hand.x,hand.y));
   var target=Grip(side+"_hand");float dx=target.X-joints[u].X,dy=target.Y-joints[u].Y,length=(float)Math.Sqrt(dx*dx+dy*dy);
   float desired=Math.Max(Math.Abs(l1-l2)+.01f,Math.Min(l1+l2-.01f,length));
   if(length<.001f){dx=1;dy=0;length=1;}
   if(Math.Abs(length-desired)<.001f)continue;
   float wx=dx*(desired/length-1),wy=dy*(desired/length-1);var weapon=Joint("weapon");var parent=joints[parents[indices["weapon"]]];
   float c=(float)Math.Cos(parent.Angle*Rad),s=(float)Math.Sin(parent.Angle*Rad);weapon.Dx+=wx*c+wy*s;weapon.Dy+=-wx*s+wy*c;UpdateWorld();
  }
 }
 void SolveArm(string upperName,string lowerName,string handName) {
  int u=indices[upperName],l=indices[lowerName],h=indices[handName];
  var a=Definition.bones[u];var b=Definition.bones[l];var c=Definition.bones[h];var target=Grip(handName);
  float dx=target.X-joints[u].X,dy=target.Y-joints[u].Y;
  float l1=(float)Math.Sqrt(DistanceSquared(a.x,a.y,b.x,b.y)),l2=(float)Math.Sqrt(DistanceSquared(b.x,b.y,c.x,c.y)),distance=(float)Math.Sqrt(dx*dx+dy*dy);
  distance=Math.Max(Math.Abs(l1-l2)+.001f,Math.Min(l1+l2-.001f,distance));
  double cosine=Math.Max(-1,Math.Min(1,(l1*l1+distance*distance-l2*l2)/(2*l1*distance)));
  float sign=(b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x)>0?-1:1;
  double angle=Math.Atan2(dy,dx)+sign*Math.Acos(cosine);
  float elbowX=joints[u].X+l1*(float)Math.Cos(angle),elbowY=joints[u].Y+l1*(float)Math.Sin(angle);
  float upperAngle=(float)((angle-Math.Atan2(b.y-a.y,b.x-a.x))/Rad);
  float lowerAngle=(float)((Math.Atan2(target.Y-elbowY,target.X-elbowX)-Math.Atan2(c.y-b.y,c.x-b.x))/Rad);
  joints[u].LocalAngle=upperAngle-joints[parents[u]].Angle;joints[l].LocalAngle=lowerAngle-upperAngle;
  joints[h].LocalAngle=Joint("weapon").Angle-lowerAngle;UpdateWorld();
 }
 public PointF HandTarget(string hand){return Grip(hand);}
 public Bitmap Render(int width,int height,float offsetX,float offsetY) {
  Evaluate();int[] output=new int[width*height],coverage=new int[width*height];int stamp=0;
  foreach(var texture in textures) {
   stamp++;
   int frame=HybridWind && Wind && texture.Pixels.Length>1?1+(int)(WindTime/Definition.frameSeconds)% (texture.Pixels.Length-1):0;
   var pixels=texture.Pixels[frame];var vertices=texture.Vertices;var transformed=new PointF[vertices.Length];
   for(int v=0;v<vertices.Length;v++) {
    var p=vertices[v];float x=0,y=0;
    for(int k=0;k<p.Bones.Length;k++){var pt=Transform(p.Bones[k],p.X,p.Y);x+=pt.X*p.Weights[k];y+=pt.Y*p.Weights[k];}
    transformed[v]=new PointF(x+offsetX,y+offsetY);
   }
   for(int i=0;i<texture.Indices.Length;i+=3) {
    int a=texture.Indices[i],b=texture.Indices[i+1],c=texture.Indices[i+2];
    Triangle(output,coverage,stamp,width,height,pixels,transformed[a],transformed[b],transformed[c],vertices[a],vertices[b],vertices[c]);
   }
  }
  var result=new Bitmap(width,height,PixelFormat.Format32bppArgb);
  var locked=result.LockBits(new Rectangle(0,0,width,height),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
  try{for(int y=0;y<height;y++)Marshal.Copy(output,y*width,IntPtr.Add(locked.Scan0,y*locked.Stride),width);}finally{result.UnlockBits(locked);}return result;
 }
 void Triangle(int[] dst,int[] coverage,int stamp,int width,int height,int[] src,PointF a,PointF b,PointF c,Vertex sa,Vertex sb,Vertex sc) {
  float det=(b.Y-c.Y)*(a.X-c.X)+(c.X-b.X)*(a.Y-c.Y);if(Math.Abs(det)<.0001f)return;
  int left=Math.Max(0,(int)Math.Floor(Math.Min(a.X,Math.Min(b.X,c.X)))),right=Math.Min(width-1,(int)Math.Ceiling(Math.Max(a.X,Math.Max(b.X,c.X))));
  int top=Math.Max(0,(int)Math.Floor(Math.Min(a.Y,Math.Min(b.Y,c.Y)))),bottom=Math.Min(height-1,(int)Math.Ceiling(Math.Max(a.Y,Math.Max(b.Y,c.Y))));
  for(int y=top;y<=bottom;y++)for(int x=left;x<=right;x++) {
   float px=x+.5f,py=y+.5f,u=((b.Y-c.Y)*(px-c.X)+(c.X-b.X)*(py-c.Y))/det,v=((c.Y-a.Y)*(px-c.X)+(a.X-c.X)*(py-c.Y))/det,w=1-u-v;
   if(u<-.00001f || v<-.00001f || w<-.00001f)continue;
   int sx=(int)Math.Floor(sa.X*u+sb.X*v+sc.X*w+.00001f),sy=(int)Math.Floor(sa.Y*u+sb.Y*v+sc.Y*w+.00001f);
   if(sx<0 || sx>=Definition.width || sy<0 || sy>=Definition.height)continue;
   uint color=(uint)src[sy*Definition.width+sx];int alpha=(int)(color>>24);if(alpha==0)continue;
   int index=y*width+x;if(coverage[index]==stamp)continue;coverage[index]=stamp;if(alpha==255){dst[index]=(int)color;continue;}
   uint old=(uint)dst[index];int oa=(int)(old>>24),outAlpha=alpha+oa*(255-alpha)/255;
   if(outAlpha==0)continue;
   int red=((int)(color>>16&255)*alpha+(int)(old>>16&255)*oa*(255-alpha)/255)/outAlpha;
   int green=((int)(color>>8&255)*alpha+(int)(old>>8&255)*oa*(255-alpha)/255)/outAlpha;
   int blue=((int)(color&255)*alpha+(int)(old&255)*oa*(255-alpha)/255)/outAlpha;
   dst[index]=outAlpha<<24|red<<16|green<<8|blue;
  }
 }
}
