using System;using System.Drawing;using System.Linq;
public static class HallSeats {
 public const int Count=19;
 // Existing seat ids 0..3 retain the original four customers' chairs.
 static readonly PointF[] hall={new PointF(529,588),new PointF(466,721),new PointF(766,689),new PointF(736,823),new PointF(641,552),new PointF(592,687),new PointF(888,655),new PointF(863,789),new PointF(518,551),new PointF(450,686),new PointF(752,650),new PointF(720,784),new PointF(640,592),new PointF(587,730),new PointF(876,694),new PointF(858,830),new PointF(709,494),new PointF(760,512),new PointF(810,525)};
 static readonly PointF[] world={new PointF(495,502),new PointF(455,583),new PointF(642,562),new PointF(625,644),new PointF(570,478),new PointF(532,559),new PointF(716,541),new PointF(701,624),new PointF(490,474),new PointF(450,552),new PointF(640,536),new PointF(619,618),new PointF(552,505),new PointF(515,585),new PointF(699,564),new PointF(683,646),new PointF(600,454),new PointF(628,465),new PointF(657,475)};
 public static int Side(int seat){return seat>=16?0:seat/4;}
 public static bool Front(int seat){return Side(seat)==1||Side(seat)==2;}
 public static bool Mirror(int seat){return Side(seat)==2||Side(seat)==3;}
 public static PointF Hip(int seat,bool panorama){return (panorama?world:hall)[seat];}
 public static PointF Approach(int seat){if(seat==15)return new PointF(910,805);var p=hall[seat];if(seat>=16)return new PointF(p.X,p.Y+65);int side=Side(seat);return new PointF(p.X+(side==0||side==2?-65:65),p.Y+(side==0||side==3?32:-32));}
 public static PointF Table(int seat,bool panorama){if(seat>=16){var p=Hip(seat,panorama);return new PointF(p.X+10,p.Y-35);}return (panorama?new[]{new PointF(525,480),new PointF(486,558),new PointF(673,540),new PointF(655,620)}:new[]{new PointF(579,550),new PointF(521,684),new PointF(816,653),new PointF(784,786)})[seat%4];}
 public static int NextFree(System.Collections.Generic.List<HallGuest> guests,int nextId){for(int offset=0;offset<Count;offset++){int seat=(nextId+offset)%Count;if(!guests.Any(g=>g.table==seat))return seat;}return -1;}
}
