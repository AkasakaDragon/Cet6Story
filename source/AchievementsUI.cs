using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

public partial class Game {
 void ShowAchievements(){
  ClearPage();page="achievements";var body=PageFlow();
  body.Controls.Add(Lab("成就",26,Gold));
  body.Controls.Add(Lab("冒险中的每一步，都值得记下。",12,Muted));
  int chaptersDone=save.completed.Distinct().Count(),stars=save.sectionStars.Values.Count(v=>v>=3),heard=save.heardLines.Distinct().Count();
  var goals=new[]{
   new{Title="启程",Description="开始你的冒险。",Progress=save.hasGame?1:0,Target=1},
   new{Title="初章落幕",Description="完成一个剧情章节。",Progress=chaptersDone,Target=1},
   new{Title="旅途渐深",Description="完成三个剧情章节。",Progress=chaptersDone,Target=3},
   new{Title="三星答卷",Description="一个章节获得三星评价。",Progress=stars,Target=1},
   new{Title="侧耳倾听",Description="听完二十句剧情台词。",Progress=heard,Target=20}
  };
  body.Controls.Add(Lab("已达成  "+goals.Count(g=>g.Progress>=g.Target)+" / "+goals.Length,14,Accent));
  foreach(var goal in goals){bool earned=goal.Progress>=goal.Target;var card=new FlowLayoutPanel{AutoSize=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,BackColor=PanelColor,Padding=new Padding(16),Margin=new Padding(6,7,6,7)};
   card.Controls.Add(Lab((earned?"已达成  ·  ":"未达成  ·  ")+goal.Title,16,earned?Gold:Muted));
   card.Controls.Add(Lab(goal.Description,12));card.Controls.Add(Lab("进度  "+Math.Min(goal.Progress,goal.Target)+" / "+goal.Target,11,earned?Accent:Muted));body.Controls.Add(card);
  }
  Action layout=()=>{foreach(var card in body.Controls.OfType<FlowLayoutPanel>()){card.MinimumSize=new Size(Math.Max(240,body.ClientSize.Width-65),0);foreach(var label in card.Controls.OfType<Label>())label.MaximumSize=new Size(Math.Max(180,card.MinimumSize.Width-45),0);}};
  body.Resize+=(s,e)=>layout();layout();
 }
}
