(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,WebSharper,Concurrency,Remoting,Html,Default,List,MnistClassification,Web,Client,EventsPervasives,HTML5,T,Unchecked,G_vmlCanvasManager,Operators;
 Runtime.Define(Global,{
  MnistClassification:{
   Web:{
    Client:{
     LoadMnist:function(k)
     {
      return Concurrency.Start(Concurrency.Delay(function()
      {
       return Concurrency.Bind(Remoting.Async("MnistClassification.Web:0",[]),function(arg101)
       {
        return Concurrency.Return(k(arg101));
       });
      }));
     },
     MnistControls:function()
     {
      var label,progress,learningRateInput,momentumInput,batchSizeInput,epochsInput,x,arg00;
      label=Default.Div(List.ofArray([Default.Text("")]));
      progress=Default.Div(List.ofArray([Default.Text("")]));
      learningRateInput=Default.Input(List.ofArray([Default.Attr().NewAttr("type","number"),Default.Attr().NewAttr("value","0.9"),Default.Attr().NewAttr("min","0.0"),Default.Attr().NewAttr("max","1.0"),Default.Attr().NewAttr("step","0.01")]));
      momentumInput=Default.Input(List.ofArray([Default.Attr().NewAttr("type","number"),Default.Attr().NewAttr("value","0.1"),Default.Attr().NewAttr("min","0.0"),Default.Attr().NewAttr("max","1.0"),Default.Attr().NewAttr("step","0.01")]));
      batchSizeInput=Default.Input(List.ofArray([Default.Attr().NewAttr("type","number"),Default.Attr().NewAttr("value","100")]));
      epochsInput=Default.Input(List.ofArray([Default.Attr().NewAttr("type","number"),Default.Attr().NewAttr("value","10"),Default.Attr().NewAttr("min","1"),Default.Attr().NewAttr("max","50")]));
      x=Default.Button(List.ofArray([Default.Text("Train MNIST Dataset")]));
      arg00=function()
      {
       return function()
       {
        label.set_Text("Fetching training set.");
        return Client.LoadMnist(function(out)
        {
         label.set_Text(out+" Starting unsupervised training.");
         return Client.TrainMnistUnsupervised(learningRateInput.get_Value(),momentumInput.get_Value(),batchSizeInput.get_Value(),epochsInput.get_Value(),function(msg)
         {
          return label.set_Text(msg);
         });
        });
       };
      };
      EventsPervasives.Events().OnClick(arg00,x);
      return Default.Div(List.ofArray([Default.Div(List.ofArray([Default.Span(List.ofArray([Default.Text("Learning Rate")])),Default.Span(List.ofArray([learningRateInput]))])),Default.Div(List.ofArray([Default.Span(List.ofArray([Default.Text("Momentum")])),Default.Span(List.ofArray([momentumInput]))])),Default.Div(List.ofArray([Default.Span(List.ofArray([Default.Text("Batch Size")])),Default.Span(List.ofArray([batchSizeInput]))])),Default.Div(List.ofArray([Default.Span(List.ofArray([Default.Text("Number of Epochs")])),Default.Span(List.ofArray([epochsInput]))])),x,label,progress]));
     },
     TrainMnistUnsupervised:function(learningRate,momentum,batchSize,epochs,k)
     {
      return Concurrency.Start(Concurrency.Delay(function()
      {
       return Concurrency.Bind(Remoting.Async("MnistClassification.Web:1",[List.ofArray([500,300,150,60,10]),learningRate,momentum,batchSize,epochs]),function(arg101)
       {
        return Concurrency.Return(k(arg101));
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
       return Client.MnistControls();
      }
     })
    }
   }
  }
 });
 Runtime.OnInit(function()
 {
  WebSharper=Runtime.Safe(Global.IntelliFactory.WebSharper);
  Concurrency=Runtime.Safe(WebSharper.Concurrency);
  Remoting=Runtime.Safe(WebSharper.Remoting);
  Html=Runtime.Safe(WebSharper.Html);
  Default=Runtime.Safe(Html.Default);
  List=Runtime.Safe(WebSharper.List);
  MnistClassification=Runtime.Safe(Global.MnistClassification);
  Web=Runtime.Safe(MnistClassification.Web);
  Client=Runtime.Safe(Web.Client);
  EventsPervasives=Runtime.Safe(Html.EventsPervasives);
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
