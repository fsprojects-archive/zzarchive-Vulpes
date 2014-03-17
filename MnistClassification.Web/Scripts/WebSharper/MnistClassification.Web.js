(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,WebSharper,Html,Default,List,MnistClassification,Web,Client,EventsPervasives,Concurrency,Remoting,HTML5,T,Unchecked,G_vmlCanvasManager,Operators;
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
      var margin,width,height,Example;
      margin={
       Top:10,
       Right:10,
       Bottom:100,
       Left:40
      };
      width=800-margin.Left-margin.Right;
      height=500-margin.Top-margin.Bottom;
      Example=function(draw)
      {
       return function(width1)
       {
        return function(height1)
        {
         return function(caption)
         {
          var _this,arg10,element,canvas;
          _this=HTML5.Tags();
          arg10=Runtime.New(T,{
           $:0
          });
          element=_this.NewTag("canvas",arg10);
          canvas=element.Body;
          if(Unchecked.Equals(canvas.getContext,undefined))
           {
            G_vmlCanvasManager.initElement(canvas);
           }
          canvas.height=height1;
          canvas.width=width1;
          draw(canvas.getContext("2d"));
          return Operators.add(Default.Div(List.ofArray([Default.Attr().NewAttr("style","float: left")])),List.ofArray([element,Operators.add(Default.P(List.ofArray([Default.Align("center")])),List.ofArray([Default.I(List.ofArray([Default.Text("Example "+caption)]))]))]));
         };
        };
       };
      };
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
  HTML5=Runtime.Safe(Default.HTML5);
  T=Runtime.Safe(List.T);
  Unchecked=Runtime.Safe(WebSharper.Unchecked);
  G_vmlCanvasManager=Runtime.Safe(Global.G_vmlCanvasManager);
  return Operators=Runtime.Safe(Html.Operators);
 });
 Runtime.OnLoad(function()
 {
  return;
 });
}());
