(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,WebSharper,Html,Default,List,MnistClassification,Web,Client,EventsPervasives,Concurrency,Remoting;
 Runtime.Define(Global,{
  MnistClassification:{
   Web:{
    Client:{
     Main:function()
     {
      var input,label,x,f,arg00;
      input=Default.Input(List.ofArray([Default.Text("")]));
      label=Default.Div(List.ofArray([Default.Text("")]));
      return Default.Div(List.ofArray([input,label,(x=Default.Button(List.ofArray([Default.Text("Click")])),(f=(arg00=function()
      {
       return function()
       {
        return Client.Start(input.get_Value(),function(out)
        {
         return label.set_Text(out);
        });
       };
      },function(arg10)
      {
       return EventsPervasives.Events().OnClick(arg00,arg10);
      }),(f(x),x)))]));
     },
     Start:function(input,k)
     {
      var arg00,clo1,t;
      arg00=Concurrency.Delay((clo1=function()
      {
       var x,f;
       x=Remoting.Async("MnistClassification.Web:0",[input]);
       f=function(_arg1)
       {
        var x1;
        x1=k(_arg1);
        return Concurrency.Return(x1);
       };
       return Concurrency.Bind(x,f);
      },clo1));
      t={
       $:0
      };
      return Concurrency.Start(arg00);
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
  return Remoting=Runtime.Safe(WebSharper.Remoting);
 });
 Runtime.OnLoad(function()
 {
 });
}());
