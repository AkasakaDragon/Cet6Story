using System;
using System.Drawing;
using System.Drawing.Drawing2D;

// Deterministic, low resolution compositing keeps every energy edge on the same pixel grid.
public static class PixelSkillVfx {
 static float Clamp(float v){return Math.Max(0,Math.Min(1,v));}
 static float Ease(float v){v=Clamp(v);return v*v*(3-2*v);}
 sealed class Ink {
  public Graphics G;public Color Edge,Core,Dark,Rune;public float Alpha;
  public void Dot(float x,float y,Color c,int size=1,float a=1){using(var b=new SolidBrush(Color.FromArgb((int)(255*Clamp(Alpha*a)),c)))G.FillRectangle(b,(int)x,(int)y,size,size);}
  public void Line(float x,float y,float xx,float yy,Color c,float a=1){int n=(int)Math.Max(Math.Abs(xx-x),Math.Abs(yy-y));for(int i=0;i<=n;i++){float t=n==0?0:i/(float)n;Dot(x+(xx-x)*t,y+(yy-y)*t,c,1,a);}}
  public void Ring(float x,float y,float r,Color c,float a=1,float flat=1){for(int i=0;i<56;i++){double t=i*Math.PI*2/56;Dot(x+(float)Math.Cos(t)*r,y+(float)Math.Sin(t)*r*flat,c,1,a);}}
  public void Glyph(float x,float y,float r,float a){Line(x,y-r,x+r,y,Rune,a);Line(x+r,y,x,y+r,Rune,a);Line(x,y+r,x-r,y,Rune,a);Line(x-r,y,x,y-r,Rune,a);Line(x-r*.45f,y-r*.45f,x+r*.45f,y+r*.45f,Core,a);Line(x+r*.45f,y-r*.45f,x-r*.45f,y+r*.45f,Core,a);Dot(x,y-r*.6f,Edge,1,a);Dot(x,y+r*.6f,Edge,1,a);}
  public void Shards(float x,float y,float radius,float t,int count){for(int i=0;i<count;i++){double angle=i*2.399;float travel=radius*(.15f+t)*(1+(i%3)*.22f);float xx=x+(float)Math.Cos(angle)*travel,yy=y+(float)Math.Sin(angle)*travel;Dot(xx,yy,i%3==0?Core:Edge,i%3==0?2:1,1-t);Line(xx,yy,xx-(float)Math.Cos(angle)*3,yy-(float)Math.Sin(angle)*3,Rune,(1-t)*.65f);}}
 }
 public static void Draw(Graphics graphics,int row,bool female,string skill,float age,float handX,float handY,float targetX,int floor,float height,bool correct){
  // Render into one coarse layer, then enlarge with nearest-neighbour. No smooth glow or gradients.
  int pixel=Math.Max(2,(int)(height/90));int width=(int)Math.Ceiling(graphics.VisibleClipBounds.Width/pixel),bottom=Math.Max(1,floor/pixel+1);
  using(var layer=new Bitmap(Math.Max(1,width),bottom))using(var g=Graphics.FromImage(layer)){
   var p=new Ink{G=g,Alpha=correct?1:.5f,Dark=female?Color.FromArgb(111,69,49):Color.FromArgb(33,53,93),Edge=female?Color.FromArgb(221,179,85):Color.FromArgb(89,182,209),Core=Color.FromArgb(239,245,222),Rune=female?Color.FromArgb(180,121,62):Color.FromArgb(150,113,193)};
   if(!female&&(skill=="detonate"||skill=="wave"||skill=="silence")){p.Edge=Color.FromArgb(160,125,207);p.Rune=Color.FromArgb(92,170,201);}
   if(female&&(skill=="bleed"||skill=="execute"))p.Rune=Color.FromArgb(176,81,76);
   float x=handX/pixel,y=handY/pixel,tx=targetX/pixel,h=height/pixel,ground=floor/(float)pixel;
   if(row==0){
    float charge=Ease(age/390);if(age<480){p.Ring(x,y,2+charge*9,p.Dark,.8f);p.Ring(x,y,2+charge*6,p.Edge);p.Glyph(x,y,3+charge*4,charge);for(int i=0;i<5;i++){double a=i*1.257+age/240;float r=4+18*(1-charge);p.Line(x+(float)Math.Cos(a)*r,y+(float)Math.Sin(a)*r,x+(float)Math.Cos(a)*r*.65f,y+(float)Math.Sin(a)*r*.65f,p.Edge,charge);}}
    if(age>=410&&age<770){float t=Ease((age-440)/290),end=Math.Max(x+h*.45f,tx),head=x+(end-x)*t;float fade=age>730?Clamp((770-age)/40):1;
     for(int ghost=2;ghost>=0;ghost--){float tip=head-ghost*6;for(int k=0;k<28;k++){float spread=(1-k/28f)*(ghost==0?4:3);p.Dot(Math.Max(x,tip-k),y-spread+(k%4==0?-1:0),p.Dark,2,fade/(ghost+1));p.Dot(Math.Max(x,tip-k),y+spread,p.Edge,1,fade/(ghost+1));if(ghost==0)p.Dot(Math.Max(x,tip-k),y,p.Core,k<8?2:1,fade);}}
     // Three broken prongs and branching electric edges, never a filled triangle.
     p.Line(head-9,y,head+6,y-3,p.Edge,fade);p.Line(head-7,y,head+4,y+4,p.Edge,fade);p.Line(head-4,y,head+10,y,p.Core,fade);
     for(int i=0;i<5;i++){float xx=Math.Max(x,head-i*7);float sign=i%2==0?1:-1;p.Line(xx,y+sign*3,xx-3,y+sign*7,p.Rune,fade*.8f);p.Line(xx-3,y+sign*7,xx-6,y+sign*5,p.Edge,fade);p.Dot(xx-4,y+sign*10,p.Edge,2,fade*.65f);}
     if(female){for(int i=0;i<18;i++){double a=-1.1+i*.12;p.Dot(head-9+(float)Math.Cos(a)*14,y+(float)Math.Sin(a)*14,p.Core,2,fade);}}
     if(age<505)p.Ring(x,y,4+(age-410)/7,p.Edge,Clamp((505-age)/95));
    }
    if(age>=740){float t=Clamp((age-740)/310);float impact=tx;float punch=age<790?1.3f:1;
     if(age<815){p.Line(impact-9*punch,y,impact+9*punch,y,p.Core,1-t);p.Line(impact,y-12*punch,impact,y+12*punch,p.Core,1-t);p.Glyph(impact,y,7,1-t);}
     p.Ring(impact,y,3+24*Ease(t),p.Edge,1-t);p.Ring(impact,y,1+17*Ease(t),p.Rune,(1-t)*.6f);p.Shards(impact,y,20,t,11);
    }
   }else if(row==1){
    float t=Ease(age/550),fade=age>720?Clamp((1050-age)/330):1;float radius=14+5*t+(age>700?(age-700)/14:0);float cx=x-7,cy=y+12;
    p.Ring(cx,cy,radius,p.Dark,fade,.65f);p.Ring(cx,cy,radius+2,p.Rune,fade*.65f,.65f);
    for(int i=0;i<6;i++){float appear=Clamp((age-i*55)/170);double a=i*Math.PI/3+Math.Sin(age/340+i)*.13;float xx=cx+(float)Math.Cos(a)*radius,yy=cy+(float)Math.Sin(a)*radius*.65f+(float)Math.Sin(age/180+i)*2;float depth=.6f+.4f*(float)(Math.Sin(a)+1)/2;p.Glyph(xx,yy,3+depth*2,appear*fade);if(age>420&&age<730&&i%2==(int)(age/90)%2)p.Line(xx,yy,cx,cy,p.Core,fade*.55f);}
    if(age>=590&&age<750){p.Glyph(tx,y,7+Ease((age-590)/160)*4,fade);p.Ring(tx,y,12,p.Rune,fade);}
    if(age>700)p.Shards(cx,cy,20,Clamp((age-700)/350),12);
    if(age>620&&age<690){p.Ring(cx,cy,radius+4,p.Core,.8f,.65f);p.Glyph(tx,y,11,1);}
   }else{
    float form=Ease((age-100)/340),fade=age>770?Clamp((1050-age)/280):1;float cx=tx+h*.27f,cy=ground-h*.56f,r=h*.28f*form;
    p.Glyph(x,y,3+form*3,age<430?1:Clamp((600-age)/170));
    for(int ring=0;ring<3;ring++){float rr=r-ring;for(int i=0;i<6;i++){double a=-Math.PI/2+i*Math.PI/3,b=a+Math.PI/3;p.Line(cx+(float)Math.Cos(a)*rr,cy+(float)Math.Sin(a)*rr*1.4f,cx+(float)Math.Cos(b)*rr,cy+(float)Math.Sin(b)*rr*1.4f,ring==0?p.Core:ring==1?p.Edge:p.Dark,fade*(ring==0?.85f:.6f));}}
    for(int i=-2;i<=2;i++)for(int j=-2;j<=2;j++){if(Math.Abs(i)+Math.Abs(j)>3)continue;float lit=Clamp((age-220-(i+j+4)*28)/100)*fade;float xx=cx+i*5,yy=cy+j*7;p.Ring(xx,yy,3,p.Edge,lit*.35f);}
    p.Glyph(cx,cy,6,fade*.8f);
    for(int i=0;i<8;i++){float yy=cy+r*1.2f-((age/35+i*7)%(Math.Max(1,r*2.4f)));p.Dot(cx+(i%2==0?-r:r),yy,p.Edge,1,fade);}
    if(age>770)p.Shards(cx,cy,r*.8f,Clamp((age-770)/280),14);
   }
   var state=graphics.Save();graphics.InterpolationMode=InterpolationMode.NearestNeighbor;graphics.PixelOffsetMode=PixelOffsetMode.Half;graphics.DrawImage(layer,new Rectangle(0,0,layer.Width*pixel,layer.Height*pixel),0,0,layer.Width,layer.Height,GraphicsUnit.Pixel);graphics.Restore(state);
  }
 }
}
