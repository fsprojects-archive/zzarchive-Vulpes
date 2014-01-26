(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,Formlet,Base,Formlet1,Form,Tree,Edit,Result,WebSharper,List,T,LayoutUtils,Tree1,Util,Seq,Enumerator,Unchecked;
 Runtime.Define(Global,{
  IntelliFactory:{
   Formlet:{
    Base:{
     D:Runtime.Class({
      Dispose:function()
      {
       return null;
      }
     },{
      New:function()
      {
       var r;
       r=Runtime.New(this,{});
       return r;
      }
     }),
     Form:Runtime.Class({
      Dispose:function()
      {
       return this.Dispose1.call(null,null);
      }
     }),
     Formlet:Runtime.Class({
      Build:function()
      {
       return this.Build1.call(null,null);
      },
      MapResult:function(f)
      {
       var _this=this;
       return Runtime.New(Formlet1,{
        Layout:this.Layout,
        Build1:function()
        {
         var form,objectArg,arg00,state;
         form=_this.Build1.call(null,null);
         objectArg=_this.Utils.Reactive;
         (arg00=form.State,function(arg10)
         {
          return objectArg.Select(arg00,arg10);
         })(f);
         state=form.State;
         return Runtime.New(Form,{
          Body:form.Body,
          Dispose1:form.Dispose1,
          Notify:form.Notify,
          State:state
         });
        },
        Utils:_this.Utils
       });
      },
      get_Layout:function()
      {
       return this.Layout;
      }
     }),
     FormletBuilder:Runtime.Class({
      Bind:function(x,f)
      {
       var objectArg;
       objectArg=this.F;
       return function(arg10)
       {
        return objectArg.Bind(x,arg10);
       }(f);
      },
      Delay:function(f)
      {
       return this.F.Delay(f);
      },
      Return:function(x)
      {
       return this.F.Return(x);
      },
      ReturnFrom:function(f)
      {
       return f;
      }
     },{
      New:function(F)
      {
       var r;
       r=Runtime.New(this,{});
       r.F=F;
       return r;
      }
     }),
     FormletProvider:Runtime.Class({
      AppendLayout:function(layout,formlet)
      {
       var x,f,_this=this;
       x=this.ApplyLayout(formlet);
       f=function(arg10)
       {
        return _this.WithLayout(layout,arg10);
       };
       return f(x);
      },
      Apply:function(f,x)
      {
       var _this=this;
       return this.New(function()
       {
        var f1,x1,body,left,objectArg,arg00,right,objectArg1,arg001,objectArg2,state,objectArg3,arg002;
        f1=_this.BuildForm(f);
        x1=_this.BuildForm(x);
        body=(left=(objectArg=_this.U.Reactive,(arg00=f1.Body,function(arg10)
        {
         return objectArg.Select(arg00,arg10);
        })(function(arg0)
        {
         return Runtime.New(Edit,{
          $:1,
          $0:arg0
         });
        })),(right=(objectArg1=_this.U.Reactive,(arg001=x1.Body,function(arg10)
        {
         return objectArg1.Select(arg001,arg10);
        })(function(arg0)
        {
         return Runtime.New(Edit,{
          $:2,
          $0:arg0
         });
        })),(objectArg2=_this.U.Reactive,function(arg10)
        {
         return objectArg2.Merge(left,arg10);
        }(right))));
        state=(objectArg3=_this.U.Reactive,((arg002=x1.State,function(arg10)
        {
         return function(arg20)
         {
          return objectArg3.CombineLatest(arg002,arg10,arg20);
         };
        })(f1.State))(function(r)
        {
         return function(f2)
         {
          return Result.Apply(f2,r);
         };
        }));
        return Runtime.New(Form,{
         Body:body,
         Dispose1:function()
         {
          x1.Dispose1.call(null,null);
          return f1.Dispose1.call(null,null);
         },
         Notify:function(o)
         {
          x1.Notify.call(null,o);
          return f1.Notify.call(null,o);
         },
         State:state
        });
       });
      },
      ApplyLayout:function(formlet)
      {
       var _this=this;
       return this.New(function()
       {
        var form,body,matchValue,disp,body1;
        form=formlet.Build();
        body=(matchValue=formlet.get_Layout().Apply.call(null,form.Body),matchValue.$==0?form.Body:(disp=matchValue.$0[1],(body1=matchValue.$0[0],_this.U.Reactive.Return(Tree.Set(body1)))));
        return Runtime.New(Form,{
         Body:body,
         Dispose1:form.Dispose1,
         Notify:form.Notify,
         State:form.State
        });
       });
      },
      Bind:function(formlet,f)
      {
       var arg00,f1,_this=this;
       arg00=(f1=function(arg10)
       {
        return _this.Map(f,arg10);
       },f1(formlet));
       return _this.Join(arg00);
      },
      BindWith:function(hF,formlet,f)
      {
       var _this=this;
       return this.New(function()
       {
        var formlet1,form,left,x,objectArg,arg00,f1,right,x1,objectArg1,arg001,f2,combB,matchValue,bLeft,bRight,x2,x3,f3,f4,objectArg2;
        formlet1=_this.Bind(formlet,f);
        form=formlet1.Build();
        left=(x=(objectArg=_this.U.Reactive,(arg00=form.Body,function(arg10)
        {
         return objectArg.Where(arg00,arg10);
        })(function(edit)
        {
         if(edit.$==1)
          {
           return true;
          }
         else
          {
           return false;
          }
        })),(f1=_this.U.DefaultLayout.Apply,f1(x)));
        right=(x1=(objectArg1=_this.U.Reactive,(arg001=form.Body,function(arg10)
        {
         return objectArg1.Where(arg001,arg10);
        })(function(edit)
        {
         if(edit.$==2)
          {
           return true;
          }
         else
          {
           return false;
          }
        })),(f2=_this.U.DefaultLayout.Apply,f2(x1)));
        combB=(matchValue=[left,right],matchValue[0].$==1?matchValue[1].$==1?(bLeft=matchValue[0].$0[0],(bRight=matchValue[1].$0[0],(x2=(x3=(hF(bLeft))(bRight),(f3=function(value)
        {
         return Tree.Set(value);
        },f3(x3))),(f4=(objectArg2=_this.U.Reactive,function(arg002)
        {
         return objectArg2.Return(arg002);
        }),f4(x2))))):_this.U.Reactive.Never():_this.U.Reactive.Never());
        return Runtime.New(Form,{
         Body:combB,
         Dispose1:form.Dispose1,
         Notify:form.Notify,
         State:form.State
        });
       });
      },
      BuildForm:function(formlet)
      {
       var form,matchValue,d,body;
       form=formlet.Build();
       matchValue=formlet.get_Layout().Apply.call(null,form.Body);
       if(matchValue.$==1)
        {
         d=matchValue.$0[1];
         body=matchValue.$0[0];
         return Runtime.New(Form,{
          Body:this.U.Reactive.Return(Tree.Set(body)),
          Dispose1:function()
          {
           form.Dispose1.call(null,null);
           return d.Dispose();
          },
          Notify:form.Notify,
          State:form.State
         });
        }
       else
        {
         return form;
        }
      },
      Delay:function(f)
      {
       var _this=this;
       return Runtime.New(Formlet1,{
        Layout:this.L.Delay(function()
        {
         return f(null).get_Layout();
        }),
        Build1:function()
        {
         return _this.BuildForm(f(null));
        },
        Utils:_this.U
       });
      },
      Deletable:function(formlet)
      {
       var arg10,_this=this;
       arg10=function(value)
       {
        var value1;
        if(value.$==1)
         {
          value1=value.$0;
          return _this.Return({
           $:1,
           $0:value1
          });
         }
        else
         {
          return _this.ReturnEmpty({
           $:0
          });
         }
       };
       return _this.Replace(formlet,arg10);
      },
      Empty:function()
      {
       var _this=this;
       return this.New(function()
       {
        return Runtime.New(Form,{
         Body:_this.U.Reactive.Return(Tree.Delete()),
         Dispose1:function(value)
         {
          value;
         },
         Notify:function(value)
         {
          value;
         },
         State:_this.U.Reactive.Never()
        });
       });
      },
      EmptyForm:function()
      {
       return Runtime.New(Form,{
        Body:this.U.Reactive.Never(),
        Dispose1:function(value)
        {
         value;
        },
        Notify:function(value)
        {
         value;
        },
        State:this.U.Reactive.Never()
       });
      },
      Fail:function(fs)
      {
       return Runtime.New(Form,{
        Body:this.U.Reactive.Never(),
        Dispose1:function(x)
        {
         return x;
        },
        Notify:function(value)
        {
         value;
        },
        State:this.U.Reactive.Return(Runtime.New(Result,{
         $:1,
         $0:fs
        }))
       });
      },
      FailWith:function(fs)
      {
       var _this=this;
       return this.New(function()
       {
        return _this.Fail(fs);
       });
      },
      FlipBody:function(formlet)
      {
       var x,_this=this,f,arg001;
       x=this.New(function()
       {
        var form,body,objectArg,arg00;
        form=formlet.Build();
        body=(objectArg=_this.U.Reactive,(arg00=form.Body,function(arg10)
        {
         return objectArg.Select(arg00,arg10);
        })(function(edit)
        {
         return Tree.FlipEdit(edit);
        }));
        return Runtime.New(Form,{
         Body:body,
         Dispose1:form.Dispose1,
         Notify:form.Notify,
         State:form.State
        });
       });
       f=(arg001=formlet.get_Layout(),function(arg10)
       {
        return _this.WithLayout(arg001,arg10);
       });
       return f(x);
      },
      FromState:function(state)
      {
       var _this=this;
       return this.New(function()
       {
        return Runtime.New(Form,{
         Body:_this.U.Reactive.Never(),
         Dispose1:function(value)
         {
          value;
         },
         Notify:function(value)
         {
          value;
         },
         State:state
        });
       });
      },
      InitWith:function(value,formlet)
      {
       var x,_this=this,f,arg001;
       x=this.New(function()
       {
        var form,state,objectArg,arg00;
        form=formlet.Build();
        state=(objectArg=_this.U.Reactive,(arg00=_this.U.Reactive.Return(Runtime.New(Result,{
         $:0,
         $0:value
        })),function(arg10)
        {
         return objectArg.Concat(arg00,arg10);
        })(form.State));
        return Runtime.New(Form,{
         Body:form.Body,
         Dispose1:form.Dispose1,
         Notify:form.Notify,
         State:state
        });
       });
       f=(arg001=formlet.get_Layout(),function(arg10)
       {
        return _this.WithLayout(arg001,arg10);
       });
       return f(x);
      },
      InitWithFailure:function(formlet)
      {
       var x,_this=this,f,arg001;
       x=this.New(function()
       {
        var form,state,objectArg,arg00;
        form=formlet.Build();
        state=(objectArg=_this.U.Reactive,(arg00=_this.U.Reactive.Return(Runtime.New(Result,{
         $:1,
         $0:Runtime.New(T,{
          $:0
         })
        })),function(arg10)
        {
         return objectArg.Concat(arg00,arg10);
        })(form.State));
        return Runtime.New(Form,{
         Body:form.Body,
         Dispose1:form.Dispose1,
         Notify:form.Notify,
         State:state
        });
       });
       f=(arg001=formlet.get_Layout(),function(arg10)
       {
        return _this.WithLayout(arg001,arg10);
       });
       return f(x);
      },
      Join:function(formlet)
      {
       var _this=this;
       return this.New(function()
       {
        var form1,formStream,x,objectArg,arg00,f,objectArg1,body,right,value,objectArg2,objectArg4,arg002,objectArg5,arg003,objectArg6,arg004,state,objectArg7,notify,dispose;
        form1=_this.BuildForm(formlet);
        formStream=(x=(objectArg=_this.U.Reactive,(arg00=form1.State,function(arg10)
        {
         return objectArg.Select(arg00,arg10);
        })(function(res)
        {
         var fs,innerF;
         if(res.$==1)
          {
           fs=res.$0;
           return _this.Fail(fs);
          }
         else
          {
           innerF=res.$0;
           return _this.BuildForm(innerF);
          }
        })),(f=(objectArg1=_this.U.Reactive,function(arg001)
        {
         return objectArg1.Heat(arg001);
        }),f(x)));
        body=(right=(value=(objectArg2=_this.U.Reactive,function(arg10)
        {
         return objectArg2.Select(formStream,arg10);
        }(function(f1)
        {
         var _delete,objectArg3;
         _delete=_this.U.Reactive.Return(Tree.Delete());
         objectArg3=_this.U.Reactive;
         return function(arg10)
         {
          return objectArg3.Concat(_delete,arg10);
         }(f1.Body);
        })),(objectArg4=_this.U.Reactive,(arg002=_this.U.Reactive.Switch(value),function(arg10)
        {
         return objectArg4.Select(arg002,arg10);
        })(function(arg0)
        {
         return Runtime.New(Edit,{
          $:2,
          $0:arg0
         });
        }))),(objectArg5=_this.U.Reactive,(arg003=(objectArg6=_this.U.Reactive,(arg004=form1.Body,function(arg10)
        {
         return objectArg6.Select(arg004,arg10);
        })(function(arg0)
        {
         return Runtime.New(Edit,{
          $:1,
          $0:arg0
         });
        })),function(arg10)
        {
         return objectArg5.Merge(arg003,arg10);
        })(right)));
        state=_this.U.Reactive.Switch((objectArg7=_this.U.Reactive,function(arg10)
        {
         return objectArg7.Select(formStream,arg10);
        }(function(f1)
        {
         return f1.State;
        })));
        notify=function(o)
        {
         return form1.Notify.call(null,o);
        };
        dispose=function()
        {
         return form1.Dispose1.call(null,null);
        };
        return Runtime.New(Form,{
         Body:body,
         Dispose1:dispose,
         Notify:notify,
         State:state
        });
       });
      },
      LiftResult:function(formlet)
      {
       return this.MapResult(function(arg0)
       {
        return Runtime.New(Result,{
         $:0,
         $0:arg0
        });
       },formlet);
      },
      Map:function(f,formlet)
      {
       return this.MapResult(function(arg10)
       {
        return Result.Map(f,arg10);
       },formlet);
      },
      MapBody:function(f,formlet)
      {
       var layout,_this=this;
       layout={
        Apply:function(o)
        {
         var matchValue,matchValue1,d,body,d1,body1;
         matchValue=formlet.get_Layout().Apply.call(null,o);
         if(matchValue.$==0)
          {
           matchValue1=_this.U.DefaultLayout.Apply.call(null,o);
           if(matchValue1.$==0)
            {
             return{
              $:0
             };
            }
           else
            {
             d=matchValue1.$0[1];
             body=matchValue1.$0[0];
             return{
              $:1,
              $0:[f(body),d]
             };
            }
          }
         else
          {
           d1=matchValue.$0[1];
           body1=matchValue.$0[0];
           return{
            $:1,
            $0:[f(body1),d1]
           };
          }
        }
       };
       return _this.WithLayout(layout,formlet);
      },
      MapResult:function(f,formlet)
      {
       var _this=this;
       return Runtime.New(Formlet1,{
        Layout:formlet.get_Layout(),
        Build1:function()
        {
         var form,state,objectArg,arg00;
         form=formlet.Build();
         state=(objectArg=_this.U.Reactive,(arg00=form.State,function(arg10)
         {
          return objectArg.Select(arg00,arg10);
         })(f));
         return Runtime.New(Form,{
          Body:form.Body,
          Dispose1:form.Dispose1,
          Notify:form.Notify,
          State:state
         });
        },
        Utils:_this.U
       });
      },
      Never:function()
      {
       var _this=this;
       return this.New(function()
       {
        return Runtime.New(Form,{
         Body:_this.U.Reactive.Never(),
         Dispose1:function(value)
         {
          value;
         },
         Notify:function(value)
         {
          value;
         },
         State:_this.U.Reactive.Never()
        });
       });
      },
      New:function(build)
      {
       return Runtime.New(Formlet1,{
        Layout:this.L.Default(),
        Build1:build,
        Utils:this.U
       });
      },
      Replace:function(formlet,f)
      {
       var arg00,f1,arg001,_this=this;
       arg00=(f1=(arg001=f,function(arg10)
       {
        return _this.Map(arg001,arg10);
       }),f1(formlet));
       return _this.Switch(arg00);
      },
      ReplaceFirstWithFailure:function(formlet)
      {
       var x,_this=this,f,arg002;
       x=this.New(function()
       {
        var form,state,state1,objectArg,arg00,objectArg1,arg001;
        form=formlet.Build();
        state=(state1=(objectArg=_this.U.Reactive,(arg00=form.State,function(arg10)
        {
         return objectArg.Drop(arg00,arg10);
        })(1)),(objectArg1=_this.U.Reactive,(arg001=_this.U.Reactive.Return(Runtime.New(Result,{
         $:1,
         $0:Runtime.New(T,{
          $:0
         })
        })),function(arg10)
        {
         return objectArg1.Concat(arg001,arg10);
        })(state1)));
        return Runtime.New(Form,{
         Body:form.Body,
         Dispose1:form.Dispose1,
         Notify:form.Notify,
         State:state
        });
       });
       f=(arg002=formlet.get_Layout(),function(arg10)
       {
        return _this.WithLayout(arg002,arg10);
       });
       return f(x);
      },
      Return:function(x)
      {
       var _this=this;
       return this.New(function()
       {
        return Runtime.New(Form,{
         Body:_this.U.Reactive.Never(),
         Dispose1:function(x1)
         {
          return x1;
         },
         Notify:function(value)
         {
          value;
         },
         State:_this.U.Reactive.Return(Runtime.New(Result,{
          $:0,
          $0:x
         }))
        });
       });
      },
      ReturnEmpty:function(x)
      {
       var _this=this;
       return this.New(function()
       {
        return Runtime.New(Form,{
         Body:_this.U.Reactive.Return(Tree.Delete()),
         Dispose1:function(x1)
         {
          return x1;
         },
         Notify:function(value)
         {
          value;
         },
         State:_this.U.Reactive.Return(Runtime.New(Result,{
          $:0,
          $0:x
         }))
        });
       });
      },
      SelectMany:function(formlet)
      {
       var _this=this;
       return this.New(function()
       {
        var form1,formStream,x,objectArg,arg00,f,objectArg1,body,left,objectArg2,arg002,right,tag,incrTag,allBodies,objectArg3,objectArg5,state,stateStream,objectArg6,objectArg7,arg003,notify,dispose;
        form1=_this.BuildForm(formlet);
        formStream=(x=(objectArg=_this.U.Reactive,(arg00=form1.State,function(arg10)
        {
         return objectArg.Choose(arg00,arg10);
        })(function(res)
        {
         var fs,innerF,arg0;
         if(res.$==1)
          {
           fs=res.$0;
           return{
            $:0
           };
          }
         else
          {
           innerF=res.$0;
           arg0=_this.BuildForm(innerF);
           return{
            $:1,
            $0:arg0
           };
          }
        })),(f=(objectArg1=_this.U.Reactive,function(arg001)
        {
         return objectArg1.Heat(arg001);
        }),f(x)));
        body=(left=(objectArg2=_this.U.Reactive,(arg002=form1.Body,function(arg10)
        {
         return objectArg2.Select(arg002,arg10);
        })(function(arg0)
        {
         return Runtime.New(Edit,{
          $:1,
          $0:arg0
         });
        })),(right=(tag={
         contents:function(arg0)
         {
          return Runtime.New(Edit,{
           $:1,
           $0:arg0
          });
         }
        },(incrTag=function()
        {
         var f1,f2;
         f1=tag.contents;
         tag.contents=(f2=function(arg0)
         {
          return Runtime.New(Edit,{
           $:2,
           $0:arg0
          });
         },function(x1)
         {
          return f2(f1(x1));
         });
        },(allBodies=(objectArg3=_this.U.Reactive,function(arg10)
        {
         return objectArg3.Select(formStream,arg10);
        }(function(f1)
        {
         var tagLocal,objectArg4,arg001;
         incrTag(null);
         tagLocal=tag.contents;
         objectArg4=_this.U.Reactive;
         return(arg001=f1.Body,function(arg10)
         {
          return objectArg4.Select(arg001,arg10);
         })(tagLocal);
        })),_this.U.Reactive.SelectMany(allBodies)))),(objectArg5=_this.U.Reactive,function(arg10)
        {
         return objectArg5.Merge(left,arg10);
        }(right))));
        state=(stateStream=(objectArg6=_this.U.Reactive,function(arg10)
        {
         return objectArg6.Select(formStream,arg10);
        }(function(f1)
        {
         return f1.State;
        })),(objectArg7=_this.U.Reactive,(arg003=_this.U.Reactive.CollectLatest(stateStream),function(arg10)
        {
         return objectArg7.Select(arg003,arg10);
        })(function(arg001)
        {
         return Result.Sequence(arg001);
        })));
        notify=function(o)
        {
         return form1.Notify.call(null,o);
        };
        dispose=function()
        {
         return form1.Dispose1.call(null,null);
        };
        return Runtime.New(Form,{
         Body:body,
         Dispose1:dispose,
         Notify:notify,
         State:state
        });
       });
      },
      Sequence:function(fs)
      {
       var fs1,fs2,f,fComp,fRest;
       fs1=List.ofSeq(fs);
       if(fs1.$==1)
        {
         fs2=fs1.$1;
         f=fs1.$0;
         fComp=this.Return(function(v)
         {
          return function(vs)
          {
           return Runtime.New(T,{
            $:1,
            $0:v,
            $1:vs
           });
          };
         });
         fRest=this.Sequence(fs2);
         return this.Apply(this.Apply(fComp,f),fRest);
        }
       else
        {
         return this.Return(Runtime.New(T,{
          $:0
         }));
        }
      },
      Switch:function(formlet)
      {
       var _this=this;
       return this.New(function()
       {
        var formlet1,x,f,form1,formStream,x1,objectArg,arg001,f1,objectArg1,body,objectArg2,arg002,objectArg3,state,objectArg4,notify,dispose;
        formlet1=(x=_this.WithLayoutOrDefault(formlet),(f=function(arg00)
        {
         return _this.ApplyLayout(arg00);
        },f(x)));
        form1=_this.BuildForm(formlet1);
        formStream=(x1=(objectArg=_this.U.Reactive,(arg001=form1.State,function(arg10)
        {
         return objectArg.Choose(arg001,arg10);
        })(function(res)
        {
         var fs,innerF,arg0;
         if(res.$==1)
          {
           fs=res.$0;
           return{
            $:0
           };
          }
         else
          {
           innerF=res.$0;
           arg0=_this.BuildForm(innerF);
           return{
            $:1,
            $0:arg0
           };
          }
        })),(f1=(objectArg1=_this.U.Reactive,function(arg00)
        {
         return objectArg1.Heat(arg00);
        }),f1(x1)));
        body=(objectArg2=_this.U.Reactive,(arg002=form1.Body,function(arg10)
        {
         return objectArg2.Concat(arg002,arg10);
        })(_this.U.Reactive.Switch((objectArg3=_this.U.Reactive,function(arg10)
        {
         return objectArg3.Select(formStream,arg10);
        }(function(f2)
        {
         return f2.Body;
        })))));
        state=_this.U.Reactive.Switch((objectArg4=_this.U.Reactive,function(arg10)
        {
         return objectArg4.Select(formStream,arg10);
        }(function(f2)
        {
         return f2.State;
        })));
        notify=function(o)
        {
         return form1.Notify.call(null,o);
        };
        dispose=function()
        {
         return form1.Dispose1.call(null,null);
        };
        return Runtime.New(Form,{
         Body:body,
         Dispose1:dispose,
         Notify:notify,
         State:state
        });
       });
      },
      WithCancelation:function(formlet,cancelFormlet)
      {
       var f1,f2,f3,f,x,f4,arg00,_this=this;
       f1=this.Return(function(r1)
       {
        return function(r2)
        {
         var matchValue,fs,s;
         matchValue=[r1,r2];
         if(matchValue[1].$==0)
          {
           return Runtime.New(Result,{
            $:0,
            $0:{
             $:0
            }
           });
          }
         else
          {
           if(matchValue[0].$==1)
            {
             fs=matchValue[0].$0;
             return Runtime.New(Result,{
              $:1,
              $0:fs
             });
            }
           else
            {
             s=matchValue[0].$0;
             return Runtime.New(Result,{
              $:0,
              $0:{
               $:1,
               $0:s
              }
             });
            }
          }
        };
       });
       f2=this.LiftResult(formlet);
       f3=this.LiftResult(cancelFormlet);
       f=this.Apply(f1,f2);
       x=this.Apply(f,f3);
       f4=(arg00=function(arg001)
       {
        return Result.Join(arg001);
       },function(arg10)
       {
        return _this.MapResult(arg00,arg10);
       });
       return f4(x);
      },
      WithLayout:function(layout,formlet)
      {
       return Runtime.New(Formlet1,{
        Layout:layout,
        Build1:function()
        {
         return formlet.Build();
        },
        Utils:this.U
       });
      },
      WithLayoutOrDefault:function(formlet)
      {
       return this.MapBody(function(x)
       {
        return x;
       },formlet);
      },
      WithNotification:function(notify,formlet)
      {
       var x,_this=this,f,arg00;
       x=this.New(function()
       {
        var form,Notify;
        form=_this.BuildForm(formlet);
        Notify=function(obj)
        {
         form.Notify.call(null,obj);
         return notify(obj);
        };
        return Runtime.New(Form,{
         Body:form.Body,
         Dispose1:form.Dispose1,
         Notify:Notify,
         State:form.State
        });
       });
       f=(arg00=formlet.get_Layout(),function(arg10)
       {
        return _this.WithLayout(arg00,arg10);
       });
       return f(x);
      },
      WithNotificationChannel:function(formlet)
      {
       var x,_this=this,f,arg002;
       x=this.New(function()
       {
        var form,state,objectArg,arg00,arg001,Notify;
        form=formlet.Build();
        state=(objectArg=_this.U.Reactive,(arg00=form.State,function(arg10)
        {
         return objectArg.Select(arg00,arg10);
        })((arg001=function(v)
        {
         return[v,form.Notify];
        },function(arg10)
        {
         return Result.Map(arg001,arg10);
        })));
        Notify=form.Notify;
        return Runtime.New(Form,{
         Body:form.Body,
         Dispose1:form.Dispose1,
         Notify:Notify,
         State:state
        });
       });
       f=(arg002=formlet.get_Layout(),function(arg10)
       {
        return _this.WithLayout(arg002,arg10);
       });
       return f(x);
      }
     },{
      New:function(U)
      {
       var r;
       r=Runtime.New(this,{});
       r.U=U;
       r.L=LayoutUtils.New({
        Reactive:r.U.Reactive
       });
       return r;
      }
     }),
     LayoutUtils:Runtime.Class({
      Default:function()
      {
       return{
        Apply:function()
        {
         return{
          $:0
         };
        }
       };
      },
      Delay:function(f)
      {
       return{
        Apply:function(x)
        {
         return f(null).Apply.call(null,x);
        }
       };
      },
      New:function(container)
      {
       return{
        Apply:function(event)
        {
         var panel,tree,disp;
         panel=container(null);
         tree={
          contents:Runtime.New(Tree1,{
           $:0
          })
         };
         disp=Util.subscribeTo(event,function(edit)
         {
          var deletedTree,patternInput,off,f,action;
          deletedTree=Tree.ReplacedTree(edit,tree.contents);
          tree.contents=Tree.Apply(edit,tree.contents);
          patternInput=Tree.Range(edit,tree.contents);
          off=patternInput[0];
          panel.Remove.call(null,deletedTree.get_Sequence());
          f=(action=function(i)
          {
           return function(e)
           {
            return(panel.Insert.call(null,off+i))(e);
           };
          },function(source)
          {
           return Seq.iteri(action,source);
          });
          return f(edit);
         });
         return{
          $:1,
          $0:[panel.Body,disp]
         };
        }
       };
      }
     },{
      New:function()
      {
       var r;
       r=Runtime.New(this,{});
       return r;
      }
     }),
     Result:Runtime.Class({},{
      Apply:function(f,r)
      {
       var matchValue,fs1,fs2,fs,fs3,f1,v;
       matchValue=[f,r];
       if(matchValue[0].$==1)
        {
         if(matchValue[1].$==1)
          {
           fs1=matchValue[0].$0;
           fs2=matchValue[1].$0;
           return Runtime.New(Result,{
            $:1,
            $0:List.append(fs1,fs2)
           });
          }
         else
          {
           fs=matchValue[0].$0;
           return Runtime.New(Result,{
            $:1,
            $0:fs
           });
          }
        }
       else
        {
         if(matchValue[1].$==1)
          {
           matchValue[0].$0;
           fs3=matchValue[1].$0;
           return Runtime.New(Result,{
            $:1,
            $0:fs3
           });
          }
         else
          {
           f1=matchValue[0].$0;
           v=matchValue[1].$0;
           return Runtime.New(Result,{
            $:0,
            $0:f1(v)
           });
          }
        }
      },
      Join:function(res)
      {
       var fs,s;
       if(res.$==1)
        {
         fs=res.$0;
         return Runtime.New(Result,{
          $:1,
          $0:fs
         });
        }
       else
        {
         s=res.$0;
         return s;
        }
      },
      Map:function(f,res)
      {
       var fs,v;
       if(res.$==1)
        {
         fs=res.$0;
         return Runtime.New(Result,{
          $:1,
          $0:fs
         });
        }
       else
        {
         v=res.$0;
         return Runtime.New(Result,{
          $:0,
          $0:f(v)
         });
        }
      },
      OfOption:function(o)
      {
       var v;
       if(o.$==0)
        {
         return Runtime.New(Result,{
          $:1,
          $0:Runtime.New(T,{
           $:0
          })
         });
        }
       else
        {
         v=o.$0;
         return Runtime.New(Result,{
          $:0,
          $0:v
         });
        }
      },
      Sequence:function(rs)
      {
       return Seq.fold(function(rs1)
       {
        return function(r)
        {
         var fs1,fs2,v,vs,fs,v1,b;
         if(rs1.$==1)
          {
           fs1=rs1.$0;
           if(r.$==1)
            {
             fs2=r.$0;
             return Runtime.New(Result,{
              $:1,
              $0:List.append(fs1,fs2)
             });
            }
           else
            {
             v=r.$0;
             return Runtime.New(Result,{
              $:1,
              $0:fs1
             });
            }
          }
         else
          {
           vs=rs1.$0;
           if(r.$==1)
            {
             fs=r.$0;
             return Runtime.New(Result,{
              $:1,
              $0:fs
             });
            }
           else
            {
             v1=r.$0;
             return Runtime.New(Result,{
              $:0,
              $0:(b=List.ofArray([v1]),List.append(vs,b))
             });
            }
          }
        };
       },Runtime.New(Result,{
        $:0,
        $0:Runtime.New(T,{
         $:0
        })
       }),rs);
      }
     }),
     Tree:{
      Apply:function(edit,input)
      {
       var apply;
       apply=function(edit1,input1)
       {
        var edit2,r,l,edit3,r1,l1,output;
        if(edit1.$==1)
         {
          edit2=edit1.$0;
          if(input1.$==2)
           {
            r=input1.$1;
            l=input1.$0;
            return Runtime.New(Tree1,{
             $:2,
             $0:apply(edit2,l),
             $1:r
            });
           }
          else
           {
            return apply(Runtime.New(Edit,{
             $:1,
             $0:edit2
            }),Runtime.New(Tree1,{
             $:2,
             $0:Runtime.New(Tree1,{
              $:0
             }),
             $1:input1
            }));
           }
         }
        else
         {
          if(edit1.$==2)
           {
            edit3=edit1.$0;
            if(input1.$==2)
             {
              r1=input1.$1;
              l1=input1.$0;
              return Runtime.New(Tree1,{
               $:2,
               $0:l1,
               $1:apply(edit3,r1)
              });
             }
            else
             {
              return apply(Runtime.New(Edit,{
               $:2,
               $0:edit3
              }),Runtime.New(Tree1,{
               $:2,
               $0:input1,
               $1:Runtime.New(Tree1,{
                $:0
               })
              }));
             }
           }
          else
           {
            output=edit1.$0;
            return output;
           }
         }
       };
       return apply(edit,input);
      },
      Count:function(t)
      {
       var loop,_;
       loop=[];
       _=Runtime.New(T,{
        $:0
       });
       loop[3]=t;
       loop[2]=_;
       loop[1]=0;
       loop[0]=1;
       Runtime.While(function()
       {
        return loop[0];
       },function()
       {
        var b,a,_1,_2,tree,k,ts,t1,_3;
        loop[3].$==2?(b=loop[3].$1,(a=loop[3].$0,(_1=Runtime.New(T,{
         $:1,
         $0:b,
         $1:loop[2]
        }),(_2=loop[1],(loop[3]=a,(loop[2]=_1,(loop[1]=_2,loop[0]=1))))))):(tree=loop[3],(k=tree.$==0?0:1,loop[2].$==1?(ts=loop[2].$1,(t1=loop[2].$0,(_3=loop[1]+k,(loop[3]=t1,(loop[2]=ts,(loop[1]=_3,loop[0]=1)))))):(loop[0]=0,loop[1]=loop[1]+k)));
       });
       return loop[1];
      },
      DeepFlipEdit:function(edit)
      {
       var e,e1,t;
       if(edit.$==1)
        {
         e=edit.$0;
         return Runtime.New(Edit,{
          $:2,
          $0:Tree.DeepFlipEdit(e)
         });
        }
       else
        {
         if(edit.$==2)
          {
           e1=edit.$0;
           return Runtime.New(Edit,{
            $:1,
            $0:Tree.DeepFlipEdit(e1)
           });
          }
         else
          {
           t=edit.$0;
           return Runtime.New(Edit,{
            $:0,
            $0:t
           });
          }
        }
      },
      Delete:function()
      {
       return Runtime.New(Edit,{
        $:0,
        $0:Runtime.New(Tree1,{
         $:0
        })
       });
      },
      Edit:Runtime.Class({
       GetEnumerator:function()
       {
        return Enumerator.Get(this.get_Sequence());
       },
       GetEnumerator1:function()
       {
        return Enumerator.Get(this.get_Sequence());
       },
       get_Sequence:function()
       {
        var edit,edit1,tree;
        if(this.$==1)
         {
          edit=this.$0;
          return edit.get_Sequence();
         }
        else
         {
          if(this.$==2)
           {
            edit1=this.$0;
            return edit1.get_Sequence();
           }
          else
           {
            tree=this.$0;
            return tree.get_Sequence();
           }
         }
       }
      }),
      FlipEdit:function(edit)
      {
       var e,e1,t;
       if(edit.$==1)
        {
         e=edit.$0;
         return Runtime.New(Edit,{
          $:2,
          $0:e
         });
        }
       else
        {
         if(edit.$==2)
          {
           e1=edit.$0;
           return Runtime.New(Edit,{
            $:1,
            $0:e1
           });
          }
         else
          {
           t=edit.$0;
           return Runtime.New(Edit,{
            $:0,
            $0:t
           });
          }
        }
      },
      FromSequence:function(vs)
      {
       var x,f,folder;
       x=Runtime.New(Tree1,{
        $:0
       });
       f=(folder=function(state)
       {
        return function(v)
        {
         return Runtime.New(Tree1,{
          $:2,
          $0:state,
          $1:Runtime.New(Tree1,{
           $:1,
           $0:v
          })
         });
        };
       },function(state)
       {
        return function(source)
        {
         return Seq.fold(folder,state,source);
        };
       });
       return(f(x))(vs);
      },
      Range:function(edit,input)
      {
       var loop;
       loop=[];
       loop[3]=0;
       loop[2]=input;
       loop[1]=edit;
       loop[0]=1;
       Runtime.While(function()
       {
        return loop[0];
       },function()
       {
        var edit1,l,_,_1,_2,edit2,r,l1,_3,tree,_4,_5;
        loop[1].$==1?(edit1=loop[1].$0,loop[2].$==2?(loop[2].$1,(l=loop[2].$0,(_=loop[3],(loop[3]=_,(loop[2]=l,(loop[1]=edit1,loop[0]=1)))))):(_1=loop[3],(_2=Runtime.New(Tree1,{
         $:0
        }),(loop[3]=_1,(loop[2]=_2,(loop[1]=edit1,loop[0]=1)))))):loop[1].$==2?(edit2=loop[1].$0,loop[2].$==2?(r=loop[2].$1,(l1=loop[2].$0,(_3=loop[3]+Tree.Count(l1),(loop[3]=_3,(loop[2]=r,(loop[1]=edit2,loop[0]=1)))))):(tree=loop[2],(_4=loop[3]+Tree.Count(tree),(_5=Runtime.New(Tree1,{
         $:0
        }),(loop[3]=_4,(loop[2]=_5,(loop[1]=edit2,loop[0]=1))))))):(loop[1].$0,(loop[0]=0,loop[1]=[loop[3],Tree.Count(loop[2])]));
       });
       return loop[1];
      },
      ReplacedTree:function(edit,input)
      {
       var edit1,l,edit2,r;
       if(edit.$==1)
        {
         edit1=edit.$0;
         if(input.$==2)
          {
           input.$1;
           l=input.$0;
           return Tree.ReplacedTree(edit1,l);
          }
         else
          {
           return Tree.ReplacedTree(Runtime.New(Edit,{
            $:1,
            $0:edit1
           }),Runtime.New(Tree1,{
            $:2,
            $0:Runtime.New(Tree1,{
             $:0
            }),
            $1:input
           }));
          }
        }
       else
        {
         if(edit.$==2)
          {
           edit2=edit.$0;
           if(input.$==2)
            {
             r=input.$1;
             input.$0;
             return Tree.ReplacedTree(edit2,r);
            }
           else
            {
             return Tree.ReplacedTree(Runtime.New(Edit,{
              $:2,
              $0:edit2
             }),Runtime.New(Tree1,{
              $:2,
              $0:input,
              $1:Runtime.New(Tree1,{
               $:0
              })
             }));
            }
          }
         else
          {
           edit.$0;
           return input;
          }
        }
      },
      Set:function(value)
      {
       return Runtime.New(Edit,{
        $:0,
        $0:Runtime.New(Tree1,{
         $:1,
         $0:value
        })
       });
      },
      ShowEdit:function(edit)
      {
       var showE;
       showE=function(edit1)
       {
        var l,r;
        if(edit1.$==1)
         {
          l=edit1.$0;
          return"Left > "+showE(l);
         }
        else
         {
          if(edit1.$==2)
           {
            r=edit1.$0;
            return"Right > "+showE(r);
           }
          else
           {
            return"Replace";
           }
         }
       };
       return showE(edit);
      },
      Transform:function(f,edit)
      {
       var e,arg0,e1,arg01,t;
       if(edit.$==1)
        {
         e=edit.$0;
         arg0=Tree.Transform(f,e);
         return Runtime.New(Edit,{
          $:1,
          $0:arg0
         });
        }
       else
        {
         if(edit.$==2)
          {
           e1=edit.$0;
           arg01=Tree.Transform(f,e1);
           return Runtime.New(Edit,{
            $:2,
            $0:arg01
           });
          }
         else
          {
           t=edit.$0;
           return Runtime.New(Edit,{
            $:0,
            $0:f(t)
           });
          }
        }
      },
      Tree:Runtime.Class({
       GetEnumerator:function()
       {
        return Enumerator.Get(this.get_Sequence());
       },
       GetEnumerator1:function()
       {
        return Enumerator.Get(this.get_Sequence());
       },
       Map:function(f)
       {
        var t,right,left;
        if(this.$==1)
         {
          t=this.$0;
          return Runtime.New(Tree1,{
           $:1,
           $0:f(t)
          });
         }
        else
         {
          if(this.$==2)
           {
            right=this.$1;
            left=this.$0;
            return Runtime.New(Tree1,{
             $:2,
             $0:left.Map(f),
             $1:right.Map(f)
            });
           }
          else
           {
            return Runtime.New(Tree1,{
             $:0
            });
           }
         }
       },
       get_Sequence:function()
       {
        var x,y,x1;
        if(this.$==1)
         {
          x=this.$0;
          return[x];
         }
        else
         {
          if(this.$==2)
           {
            y=this.$1;
            x1=this.$0;
            return Seq.append(x1.get_Sequence(),y.get_Sequence());
           }
          else
           {
            return Seq.empty();
           }
         }
       }
      })
     },
     Validator:Runtime.Class({
      Is:function(f,m,flet)
      {
       var _this=this;
       return function(arg20)
       {
        return _this.Validate(f,m,arg20);
       }(flet);
      },
      IsEmail:function(msg)
      {
       var _this=this;
       return function(arg20)
       {
        return _this.IsRegexMatch("^[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\\.)+[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?$",msg,arg20);
       };
      },
      IsEqual:function(value,msg,flet)
      {
       var _this=this;
       return function(arg20)
       {
        return _this.Validate(function(i)
        {
         return Unchecked.Equals(i,value);
        },msg,arg20);
       }(flet);
      },
      IsFloat:function(msg)
      {
       var _this=this;
       return function(arg20)
       {
        return _this.IsRegexMatch("^\\s*(\\+|-)?((\\d+(\\.\\d+)?)|(\\.\\d+))\\s*$",msg,arg20);
       };
      },
      IsGreaterThan:function(min,msg,flet)
      {
       var _this=this;
       return function(arg20)
       {
        return _this.Validate(function(i)
        {
         return Unchecked.Compare(i,min)===1;
        },msg,arg20);
       }(flet);
      },
      IsInt:function(msg)
      {
       var _this=this;
       return function(arg20)
       {
        return _this.IsRegexMatch("^-?\\d+$",msg,arg20);
       };
      },
      IsLessThan:function(max,msg,flet)
      {
       var _this=this;
       return function(arg20)
       {
        return _this.Validate(function(i)
        {
         return Unchecked.Compare(i,max)===-1;
        },msg,arg20);
       }(flet);
      },
      IsNotEmpty:function(msg,flet)
      {
       var _this=this;
       return function(arg20)
       {
        return _this.Validate(function(s)
        {
         return s!=="";
        },msg,arg20);
       }(flet);
      },
      IsNotEqual:function(value,msg,flet)
      {
       var _this=this;
       return function(arg20)
       {
        return _this.Validate(function(i)
        {
         return!Unchecked.Equals(i,value);
        },msg,arg20);
       }(flet);
      },
      IsRegexMatch:function(regex,msg,flet)
      {
       var _this=this;
       return function(arg20)
       {
        return _this.Validate(function(x)
        {
         var objectArg;
         objectArg=_this.VP;
         return function(arg10)
         {
          return objectArg.Matches(regex,arg10);
         }(x);
        },msg,arg20);
       }(flet);
      },
      IsTrue:function(msg,flet)
      {
       var _this=this;
       return function(arg20)
       {
        return _this.Validate(function(x)
        {
         return x;
        },msg,arg20);
       }(flet);
      },
      Validate:function(f,msg,flet)
      {
       var value;
       value=flet.MapResult(function(res)
       {
        var fs,v;
        if(res.$==1)
         {
          fs=res.$0;
          return Runtime.New(Result,{
           $:1,
           $0:fs
          });
         }
        else
         {
          v=res.$0;
          if(f(v))
           {
            return Runtime.New(Result,{
             $:0,
             $0:v
            });
           }
          else
           {
            return Runtime.New(Result,{
             $:1,
             $0:List.ofArray([msg])
            });
           }
         }
       });
       return value;
      }
     },{
      New:function(VP)
      {
       var r;
       r=Runtime.New(this,{});
       r.VP=VP;
       return r;
      }
     })
    }
   }
  }
 });
 Runtime.OnInit(function()
 {
  Formlet=Runtime.Safe(Global.IntelliFactory.Formlet);
  Base=Runtime.Safe(Formlet.Base);
  Formlet1=Runtime.Safe(Base.Formlet);
  Form=Runtime.Safe(Base.Form);
  Tree=Runtime.Safe(Base.Tree);
  Edit=Runtime.Safe(Tree.Edit);
  Result=Runtime.Safe(Base.Result);
  WebSharper=Runtime.Safe(Global.IntelliFactory.WebSharper);
  List=Runtime.Safe(WebSharper.List);
  T=Runtime.Safe(List.T);
  LayoutUtils=Runtime.Safe(Base.LayoutUtils);
  Tree1=Runtime.Safe(Tree.Tree);
  Util=Runtime.Safe(WebSharper.Util);
  Seq=Runtime.Safe(WebSharper.Seq);
  Enumerator=Runtime.Safe(WebSharper.Enumerator);
  return Unchecked=Runtime.Safe(WebSharper.Unchecked);
 });
 Runtime.OnLoad(function()
 {
 });
}());
