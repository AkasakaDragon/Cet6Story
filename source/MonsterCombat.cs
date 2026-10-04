using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public static class MonsterCombat {
 public static int Profile(RogueRun run){if(run.enemy=="boss")return 9;if(run.enemy=="elite")return 6+Math.Max(0,Math.Min(2,run.theme));int index=Array.IndexOf(CardBattle.Monsters,run.cardBattle==null?null:run.cardBattle.monster);return index>=0?index:Math.Max(0,Math.Min(2,run.theme))*2;}
 public static string PlannedAction(RogueRun run){
  int phase=run.cardBattle==null?3:Math.Max(0,run.cardBattle.turn-1)%4;
  if(Profile(run)==2){if(phase==0)return "skill-a";if(phase==1)return "skill-b";return phase==2?"none":"normal";}
  if(Profile(run)==4&&phase==3)return "none";
  return phase==0?"skill-a":phase==2?"skill-b":"normal";
 }
 public static string Action(RogueRun run){return run.cardBattle!=null&&!String.IsNullOrEmpty(run.cardBattle.lastMonsterAction)?run.cardBattle.lastMonsterAction:run.enemyHp<=0?"none":PlannedAction(run);}
 public static string SoundKey(RogueRun run,string action){return MonsterAudio.Profiles[Profile(run)]+"/"+action;}
 static int Family(int profile){return profile<6?profile:profile==6?0:profile==7?2:profile==8?5:3;}
 static Color Palette(int profile){Color[] colors={Color.FromArgb(148,173,112),Color.FromArgb(168,132,176),Color.FromArgb(169,184,184),Color.FromArgb(125,178,204),Color.FromArgb(210,144,91),Color.FromArgb(193,152,114)};return profile==9?Color.FromArgb(189,174,114):colors[Family(profile)];}
 public static Rectangle Pose(RogueRun run,Rectangle rect,int frame){
  if(run.lastDamage>0&&frame>=16&&frame<23)rect.Offset((int)(Math.Sin((frame-16)*2.7)*5),0);
  if(Action(run)=="none"||frame<22||frame>47)return rect;
  float t=(frame-22)/25f,wave=(float)Math.Sin(t*Math.PI);string action=Action(run);
  if(action=="normal")rect.Offset(-(int)(wave*(Profile(run)==2?26:50)),0);
  else{int stretch=(int)(wave*rect.Height*(action=="skill-a"?.035:.055));rect.Y-=stretch;rect.Height+=stretch;rect.Inflate((int)(wave*3),0);}
  return rect;
 }
 public static void Draw(Graphics g,RogueRun run,Rectangle enemy,Rectangle hero,int frame){
  int profile=Profile(run),family=Family(profile);Color color=Palette(profile);var state=g.Save();g.SmoothingMode=SmoothingMode.AntiAlias;
  float ex=enemy.Left+enemy.Width*.48f,ey=enemy.Top+enemy.Height*.48f,hx=hero.Left+hero.Width*.60f,hy=hero.Top+hero.Height*.48f;
  if(run.lastDamage>0&&frame>=16&&frame<24)Particles(g,ex,ey,frame-16,color,profile+5,Math.Min(1,enemy.Height/300f));
  string action=Action(run);
  if(action!="none"&&frame>=22&&frame<=47){
   float t=(frame-22)/25f;float size=Math.Min(1,enemy.Height/300f);bool skill=action!="normal";
   if(frame<30&&skill){Ring(g,ex,enemy.Bottom-6,(32+t*90)*size,color,120);Particles(g,ex,ey,frame-22,color,profile+3,size*.5f);}
   if(action=="skill-a"){
    Cast(g,family,ex,ey,enemy.Bottom,t,size,color,profile);
    if(family==3){float wave=Math.Min(1,(frame-24)/12f);if(wave>0)Ring(g,ex+(hx-ex)*wave,hy,35*size,color,130);}
   }
   else if(frame>=28){float travel=Math.Min(1,(frame-28)/9f);float x=ex+(hx-ex)*travel,y=ey+(hy-ey)*travel;float power=action=="skill-b"?1.8f:1;
    if(family==0){using(var pen=new Pen(Color.FromArgb(190,color),5*size*power)){var points=new[]{new PointF(ex,ey),new PointF(ex-(ex-hx)*.35f,ey+40*size),new PointF(x,y)};g.DrawCurve(pen,points);}Leaves(g,x,y,frame,color,6,size*power);}
    else if(family==1){for(int i=0;i<(skill?9:4);i++){float offset=(i-3)*8*size;Orb(g,x+offset,y+(float)Math.Sin(i+travel*5)*20*size,5*size,color);}if(travel>=1)Particles(g,hx,hy,frame-36,color,profile+11,size*power);}
    else if(family==2){for(int i=0;i<(skill?6:3);i++){float k=(i-2)*13*size;using(var brush=new SolidBrush(Color.FromArgb(185,color)))g.FillPolygon(brush,new[]{new PointF(x+k,y-10*size),new PointF(x+k+13*size,y),new PointF(x+k+3*size,y+12*size),new PointF(x+k-7*size,y+4*size)});}if(travel>=1)Ring(g,hx,hero.Bottom-4,(15+(frame-36)*7)*size,color,155);}
    else if(family==3){Orb(g,x,y,(skill?22:12)*size,color);Ring(g,x,y,22*size*power,color,120);if(skill)Ring(g,x,y,34*size,color,95);}
    else if(family==4){for(int i=0;i<9;i++)Orb(g,x+i*8*size,y+(float)Math.Sin(i+t*12)*8*size,(13-i)*size*power,color);}
    else{using(var pen=new Pen(Color.FromArgb(185,color),4*size*power)){g.DrawLine(pen,x-18*size,y+25*size,x+12*size,y-20*size);g.DrawLine(pen,x-4*size,y+30*size,x+25*size,y-14*size);}if(skill)Particles(g,x,y,frame-28,color,profile+7,size);}
    if(action=="skill-b"&&frame>=36){
     float burst=(frame-36)/11f;
     if(family==0){float returning=burst;for(int i=0;i<5;i++)Orb(g,hx+(ex-hx)*returning,hy+(ey-hy)*returning+(i-2)*10*size,5*size,color);}
     if(family==1)for(int i=0;i<14;i++){double angle=i*Math.PI*2/14;Orb(g,hx+(float)Math.Cos(angle)*(18+burst*65)*size,hy+(float)Math.Sin(angle)*(18+burst*65)*size,5*size,color);}
     if(family==2||family==5){Ring(g,hx,hero.Bottom-4,(20+burst*110)*size,color,140);Ring(g,hx,hero.Bottom-4,(10+burst*75)*size,color,85);}
     if(profile==9){Polygon(g,hx,hy,(20+burst*75)*size,color,8,burst);Ring(g,hx,hy,(30+burst*90)*size,color,120);}
    }
    if(profile==7){using(var blade=new Pen(Color.FromArgb(150,color),5*size))g.DrawArc(blade,x-35*size,y-50*size,70*size,100*size,120,130);}
    if(frame>=36){if(run.cardBattle!=null&&run.cardBattle.lastEvaded){using(var font=GameTheme.Body(11,FontStyle.Bold))TextRenderer.DrawText(g,"闪避",font,new Rectangle((int)hx-45,(int)hy-45-(frame-36)*2,90,28),Color.FromArgb(196,223,211),TextFormatFlags.HorizontalCenter);}
     else if(run.lastReceived>0||run.cardBattle!=null&&run.cardBattle.lastBlocked>0){Particles(g,hx,hy,frame-36,color,profile+13,size*power);if(run.cardBattle!=null&&run.cardBattle.lastBlocked>0)Ring(g,hx,hy,hero.Height*.34f,color,150);}
    }
   }
  }
  g.Restore(state);
 }
 static void Cast(Graphics g,int family,float x,float y,float floor,float t,float scale,Color color,int profile){
  float radius=(30+70*(float)Math.Sin(t*Math.PI))*scale;
  switch(family){
   case 0:for(int i=0;i<5;i++){float root=x+(i-2)*20*scale;using(var pen=new Pen(Color.FromArgb(170,color),4*scale))g.DrawBezier(pen,root,floor,root-20*scale,floor-35*scale,root+25*scale,floor-55*scale,root,floor-radius);}Leaves(g,x,y,(int)(t*30),color,10,scale);break;
   case 1:for(int i=0;i<14;i++){double a=i*2.4+t*4;Orb(g,x+(float)Math.Cos(a)*radius,y+(float)Math.Sin(a)*radius*.6f,4*scale,color);}break;
   case 2:Polygon(g,x,y,radius,color,6,t);Polygon(g,x,y,radius*.64f,color,4,-t);break;
   case 3:for(int i=0;i<3;i++)Ring(g,x,y,radius+i*15*scale,color,145-i*30);if(profile==9)Polygon(g,x,y,radius*1.15f,color,8,t);break;
   case 4:for(int i=0;i<7;i++){float dx=(i-3)*16*scale;Orb(g,x+dx,floor-25*scale-(float)Math.Sin(t*Math.PI+i*.4)*55*scale,12*scale,color);}break;
   default:Polygon(g,x,y,radius,color,6,t*.3f);Ring(g,x,y,radius*.8f,color,150);break;
  }
 }
 static void Polygon(Graphics g,float x,float y,float radius,Color color,int sides,float rotation){var points=new PointF[sides];for(int i=0;i<sides;i++){double a=i*Math.PI*2/sides+rotation;points[i]=new PointF(x+(float)Math.Cos(a)*radius,y+(float)Math.Sin(a)*radius);}using(var pen=new Pen(Color.FromArgb(165,color),2))g.DrawPolygon(pen,points);}
 static void Ring(Graphics g,float x,float y,float radius,Color color,int alpha){if(radius<=0)return;using(var pen=new Pen(Color.FromArgb(alpha,color),2))g.DrawEllipse(pen,x-radius,y-radius*.55f,radius*2,radius*1.1f);}
 static void Orb(Graphics g,float x,float y,float radius,Color color){if(radius<=0)return;for(int i=2;i>=0;i--)using(var brush=new SolidBrush(Color.FromArgb(i==0?165:30,color)))g.FillEllipse(brush,x-radius-i*3,y-radius-i*3,radius*2+i*6,radius*2+i*6);}
 static void Leaves(Graphics g,float x,float y,int frame,Color color,int count,float scale){for(int i=0;i<count;i++){double a=i*2.4+frame*.12;float dx=(float)Math.Cos(a)*22*scale,dy=(float)Math.Sin(a)*18*scale;using(var brush=new SolidBrush(Color.FromArgb(175,color)))g.FillEllipse(brush,x+dx,y+dy,10*scale,4*scale);}}
 static void Particles(Graphics g,float x,float y,int age,Color color,int count,float scale){if(age<0||age>14)return;int alpha=Math.Max(0,190-age*12);using(var brush=new SolidBrush(Color.FromArgb(alpha,color)))for(int i=0;i<count;i++){double a=i*Math.PI*2/count;float distance=(8+age*5)*scale;g.FillRectangle(brush,x+(float)Math.Cos(a)*distance,y+(float)Math.Sin(a)*distance*.7f+age*age*.2f,4*scale,4*scale);}}
}
