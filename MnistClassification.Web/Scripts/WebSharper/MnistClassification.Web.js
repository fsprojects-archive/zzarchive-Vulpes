(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,WebSharper,Html,Default,List,MnistClassification,Web,Client,EventsPervasives,Concurrency,Remoting,d3,T;
 Runtime.Define(Global,{
  MnistClassification:{
   Web:{
    Client:{
     Main:function()
     {
      var input,label,x,arg00;
      input=Default.Input(List.ofArray([Default.Text("")]));
      label=Default.Div(List.ofArray([Default.Text("")]));
      x=Default.Button(List.ofArray([Default.Text("Click")]));
      arg00=function()
      {
       return function()
       {
        return Client.Start(input.get_Value(),function(out)
        {
         return label.set_Text(out);
        });
       };
      };
      EventsPervasives.Events().OnClick(arg00,x);
      return Default.Div(List.ofArray([input,label,x]));
     },
     Start:function(input,k)
     {
      return Concurrency.Start(Concurrency.Delay(function()
      {
       return Concurrency.Bind(Remoting.Async("MnistClassification.Web:0",[input]),function(_arg1)
       {
        return Concurrency.Return(k(_arg1));
       });
      }));
     },
     TrainingSet:function()
     {
      var margin,width,height;
      margin={
       Top:10,
       Right:10,
       Bottom:100,
       Left:40
      };
      width=800-margin.Left-margin.Right;
      height=500-margin.Top-margin.Bottom;
      d3.select("body").append("svg").attr("width",width+margin.Left+margin.Right).attr("height",height+margin.Top+margin.Bottom);
      return Default.Div(Runtime.New(T,{
       $:0
      }));
     }
    },
    Controls:{
     EntryPoint:Runtime.Class({
      get_Body:function()
      {
       return Client.Main();
      }
     })
    }
   }
  }
 });
 Runtime.OnInit(function()
 {
  WebSharper=Runtime.Safe(Global.IntelliFactory.WebSharper);
  Html=Runtime.Safe(WebSharper.Html);
  Default=Runtime.Safe(Html.Default);
  List=Runtime.Safe(WebSharper.List);
  MnistClassification=Runtime.Safe(Global.MnistClassification);
  Web=Runtime.Safe(MnistClassification.Web);
  Client=Runtime.Safe(Web.Client);
  EventsPervasives=Runtime.Safe(Html.EventsPervasives);
  Concurrency=Runtime.Safe(WebSharper.Concurrency);
  Remoting=Runtime.Safe(WebSharper.Remoting);
  d3=Runtime.Safe(Global.d3);
  return T=Runtime.Safe(List.T);
 });
 Runtime.OnLoad(function()
 {
  return;
 });
}());
