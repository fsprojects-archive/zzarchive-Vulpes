(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,WebSharper,Formlet,Body,Controls,Html,Default,List,Data,Reactive,HotStream,Formlet1,Base,Result,T,Operators,jQuery,EventsPervasives,Formlet2,Operators1,CssConstants,Math,Seq,Utils,Tree,Edit,Form,Arrays,FormletProvider,Formlet3,Util,LayoutProvider,LayoutUtils,Reactive1,Validator,ValidatorProvidor,RegExp,Collections,Dictionary,ElementStore,Enhance,FormButtonConfiguration,FormContainerConfiguration,Padding,ManyConfiguration,ValidationFrameConfiguration,ValidationIconConfiguration,JSON,FormletBuilder,Layout,FormRowConfiguration,LabelConfiguration,Padding1,Enumerator;
 Runtime.Define(Global,{
  IntelliFactory:{
   WebSharper:{
    Formlet:{
     Body:Runtime.Class({},{
      New:function(el,l)
      {
       return Runtime.New(Body,{
        Element:el,
        Label:l
       });
      }
     }),
     Controls:{
      Button:function(label)
      {
       return Controls.ElementButton(function()
       {
        return Default.Button(List.ofArray([Default.Text(label)]));
       });
      },
      Checkbox:function(def)
      {
       return Controls.CheckboxControl(false,def);
      },
      CheckboxControl:function(readOnly,def)
      {
       return Data.MkFormlet(function()
       {
        var state,body,readOnlyAtts,_this,x,_this1,f,arg00,objectArg,arg002,objectArg1,arg003,reset;
        state=HotStream.New(Runtime.New(Result,{
         $:0,
         $0:def
        }));
        body=(readOnlyAtts=readOnly?List.ofArray([(_this=Default.Attr(),_this.NewAttr("disabled","disabled"))]):Runtime.New(T,{
         $:0
        }),(x=Operators.add(Default.Input(List.ofArray([(_this1=Default.Attr(),_this1.NewAttr("type","checkbox")),Default.Attr().Class("inputCheckbox")])),readOnlyAtts),(f=(arg00=function(cb)
        {
         return function()
         {
          var x1,x2,f1,f2;
          if(!readOnly)
           {
            x1=(x2=jQuery(cb.Body).prop("checked"),(f1=function(arg0)
            {
             return Runtime.New(Result,{
              $:0,
              $0:arg0
             });
            },f1(x2)));
            f2=function(arg001)
            {
             return state.Trigger(arg001);
            };
            return f2(x1);
           }
          else
           {
            return null;
           }
         };
        },function(arg10)
        {
         return EventsPervasives.Events().OnClick(arg00,arg10);
        }),(f(x),x))));
        if(def)
         {
          objectArg=body["HtmlProvider@32"];
          ((arg002=body.Body,function(arg10)
          {
           return function(arg20)
           {
            return objectArg.SetAttribute(arg002,arg10,arg20);
           };
          })("defaultChecked"))("true");
         }
        else
         {
          objectArg1=body["HtmlProvider@32"];
          (arg003=body.Body,function(arg10)
          {
           return objectArg1.RemoveAttribute(arg003,arg10);
          })("checked");
         }
        reset=function()
        {
         var objectArg2,arg001,objectArg3,arg004,objectArg4,arg005;
         if(def)
          {
           objectArg2=body["HtmlProvider@32"];
           ((arg001=body.Body,function(arg10)
           {
            return function(arg20)
            {
             return objectArg2.SetProperty(arg001,arg10,arg20);
            };
           })("checked"))(true);
          }
         else
          {
           objectArg3=body["HtmlProvider@32"];
           (arg004=body.Body,function(arg10)
           {
            return objectArg3.RemoveAttribute(arg004,arg10);
           })("checked");
           objectArg4=body["HtmlProvider@32"];
           ((arg005=body.Body,function(arg10)
           {
            return function(arg20)
            {
             return objectArg4.SetProperty(arg005,arg10,arg20);
            };
           })("checked"))(false);
          }
         return state.Trigger(Runtime.New(Result,{
          $:0,
          $0:def
         }));
        };
        reset(null);
        return[body,reset,state];
       });
      },
      CheckboxGroup:function(values)
      {
       return Controls.CheckboxGroupControl(false,values);
      },
      CheckboxGroupControl:function(readOnly,values)
      {
       var x,fs,f,mapping,f4,f5,chooser;
       x=(fs=(f=(mapping=Runtime.Tupled(function(tupledArg)
       {
        var l,v,b,x1,x2,f1,label,arg0,f2,f3;
        l=tupledArg[0];
        v=tupledArg[1];
        b=tupledArg[2];
        x1=(x2=Controls.CheckboxControl(readOnly,b),(f1=(label=(arg0=function()
        {
         var x3,_this;
         x3=List.ofArray([Default.Text(l)]);
         _this=Default.Tags();
         return _this.NewTag("label",x3);
        },{
         $:1,
         $0:arg0
        }),function(formlet)
        {
         return Formlet2.WithLabel(label,formlet);
        }),f1(x2)));
        f2=(f3=function(b1)
        {
         return[b1,v];
        },function(formlet)
        {
         return Formlet2.Map(f3,formlet);
        });
        return f2(x1);
       }),function(list)
       {
        return List.map(mapping,list);
       }),f(values)),Formlet2.Sequence(fs));
       f4=(f5=(chooser=Runtime.Tupled(function(tupledArg)
       {
        var b,v;
        b=tupledArg[0];
        v=tupledArg[1];
        if(b)
         {
          return{
           $:1,
           $0:v
          };
         }
        else
         {
          return{
           $:0
          };
         }
       }),function(list)
       {
        return List.choose(chooser,list);
       }),function(formlet)
       {
        return Formlet2.Map(f5,formlet);
       });
       return f4(x);
      },
      ElementButton:function(genElem)
      {
       return Data.MkFormlet(function()
       {
        var state,count,body,x,f,arg00,reset;
        state=HotStream.New(Runtime.New(Result,{
         $:1,
         $0:Runtime.New(T,{
          $:0
         })
        }));
        count={
         contents:0
        };
        body=(x=genElem(null),(f=(arg00=function()
        {
         return function()
         {
          state.Trigger(Runtime.New(Result,{
           $:0,
           $0:count.contents
          }));
          return Operators1.Increment(count);
         };
        },function(arg10)
        {
         return EventsPervasives.Events().OnClick(arg00,arg10);
        }),(f(x),x)));
        reset=function()
        {
         count.contents=0;
         return state.Trigger(Runtime.New(Result,{
          $:1,
          $0:Runtime.New(T,{
           $:0
          })
         }));
        };
        return[body,reset,state];
       });
      },
      Input:function(value)
      {
       return Controls.InputField(false,"text",CssConstants.InputTextClass(),value);
      },
      InputControl:function(value,f)
      {
       return Data.MkFormlet(function()
       {
        var state,body,reset;
        state=HotStream.New(Runtime.New(Result,{
         $:0,
         $0:value
        }));
        body=f(state);
        body.set_Value(value);
        reset=function()
        {
         body.set_Value(value);
         return state.Trigger(Runtime.New(Result,{
          $:0,
          $0:value
         }));
        };
        return[body,reset,state];
       });
      },
      InputField:function(readOnly,typ,cls,value)
      {
       return Controls.InputControl(value,function(state)
       {
        var input,ro,_this,_this1,f;
        input=(ro=readOnly?List.ofArray([(_this=Default.Attr(),_this.NewAttr("readonly","readonly"))]):Runtime.New(T,{
         $:0
        }),Default.Input(List.append(List.ofArray([(_this1=Default.Attr(),_this1.NewAttr("type",typ)),Default.Attr().Class(cls)]),ro)));
        f=function(control)
        {
         return Controls.OnTextChange(function()
         {
          if(!readOnly)
           {
            return state.Trigger(Runtime.New(Result,{
             $:0,
             $0:input.get_Value()
            }));
           }
          else
           {
            return null;
           }
         },control);
        };
        f(input);
        return input;
       });
      },
      OnTextChange:function(f,control)
      {
       var value,up;
       value={
        contents:control.get_Value()
       };
       up=function()
       {
        if(control.get_Value()!==value.contents)
         {
          value.contents=control.get_Value();
          return f(null);
         }
        else
         {
          return null;
         }
       };
       EventsPervasives.Events().OnChange(function()
       {
        return up(null);
       },control);
       EventsPervasives.Events().OnKeyUp(function()
       {
        return function()
        {
         return up(null);
        };
       },control);
       control.Body.oninput=up;
      },
      Password:function(value)
      {
       return Controls.InputField(false,"password","inputPassword",value);
      },
      RadioButtonGroup:function(def,values)
      {
       return Controls.RadioButtonGroupControl(false,def,values);
      },
      RadioButtonGroupControl:function(readOnly,def,values)
      {
       return Formlet2.New(function()
       {
        var groupId,state,x,defIx,x1,f,mapping,f1,chooser,f2,d,f3,rbLbVls,f4,mapping1,resetRB,reset,body,x2,x3,x4,f6,mapping2,f7,f8,f9;
        groupId="id"+Math.round(Math.random()*100000000);
        state=(x=def.$==0?{
         $:0
        }:(defIx=def.$0,(x1=(f=(mapping=function(ix)
        {
         return Runtime.Tupled(function(tupledArg)
         {
          var _arg1,value;
          _arg1=tupledArg[0];
          value=tupledArg[1];
          return[ix,value];
         });
        },function(list)
        {
         return List.mapi(mapping,list);
        }),f(values)),(f1=(chooser=Runtime.Tupled(function(tupledArg)
        {
         var ix,value,defIx1;
         ix=tupledArg[0];
         value=tupledArg[1];
         if(def.$==0)
          {
           return{
            $:0
           };
          }
         else
          {
           defIx1=def.$0;
           if(defIx1===ix)
            {
             return{
              $:1,
              $0:Runtime.New(Result,{
               $:0,
               $0:value
              })
             };
            }
           else
            {
             return{
              $:0
             };
            }
          }
        }),function(list)
        {
         return Seq.tryPick(chooser,list);
        }),f1(x1)))),(f2=(d=HotStream.New(Runtime.New(Result,{
         $:1,
         $0:Runtime.New(T,{
          $:0
         })
        })),(f3=function(arg00)
        {
         return HotStream.New(arg00);
        },function(o)
        {
         return Utils.Maybe(d,f3,o);
        })),f2(x)));
        rbLbVls=(f4=(mapping1=Runtime.Tupled(function(tupledArg)
        {
         var label,value,inp,_this,_this1,_this2;
         label=tupledArg[0];
         value=tupledArg[1];
         inp=Operators.add(Default.Input(List.ofArray([Default.Attr().Class("inputRadio"),(_this=Default.Attr(),_this.NewAttr("type","radio")),(_this1=Default.Attr(),_this1.NewAttr("name",groupId))])),readOnly?List.ofArray([(_this2=Default.Attr(),_this2.NewAttr("disabled","disabled"))]):Runtime.New(T,{
          $:0
         }));
         return[inp,label,value];
        }),function(list)
        {
         return List.map(mapping1,list);
        }),f4(values));
        resetRB=function(rb,value,ix)
        {
         var objectArg,arg00,defIx1,objectArg1,arg001,objectArg2,arg002;
         if(def.$==0)
          {
           objectArg=rb["HtmlProvider@32"];
           (arg00=rb.Body,function(arg10)
           {
            return objectArg.RemoveAttribute(arg00,arg10);
           })("checked");
           return state.Trigger(Runtime.New(Result,{
            $:1,
            $0:Runtime.New(T,{
             $:0
            })
           }));
          }
         else
          {
           defIx1=def.$0;
           if(defIx1===ix)
            {
             objectArg1=rb["HtmlProvider@32"];
             ((arg001=rb.Body,function(arg10)
             {
              return function(arg20)
              {
               return objectArg1.SetProperty(arg001,arg10,arg20);
              };
             })("checked"))(true);
             return state.Trigger(Runtime.New(Result,{
              $:0,
              $0:value
             }));
            }
           else
            {
             objectArg2=rb["HtmlProvider@32"];
             return((arg002=rb.Body,function(arg10)
             {
              return function(arg20)
              {
               return objectArg2.SetProperty(arg002,arg10,arg20);
              };
             })("checked"))(false);
            }
          }
        };
        reset=function()
        {
         var f5,action;
         f5=(action=function(ix)
         {
          return Runtime.Tupled(function(tupledArg)
          {
           var rb,_arg2,value;
           rb=tupledArg[0];
           _arg2=tupledArg[1];
           value=tupledArg[2];
           return resetRB(rb,value,ix);
          });
         },function(list)
         {
          return Seq.iteri(action,list);
         });
         return f5(rbLbVls);
        };
        body=(x2=(x3=(x4=(f6=(mapping2=function(ix)
        {
         return Runtime.Tupled(function(tupledArg)
         {
          var rb,label,value,f5,arg00,Label;
          rb=tupledArg[0];
          label=tupledArg[1];
          value=tupledArg[2];
          resetRB(rb,value,ix);
          f5=(arg00=function()
          {
           return function()
           {
            if(!readOnly)
             {
              return state.Trigger(Runtime.New(Result,{
               $:0,
               $0:value
              }));
             }
            else
             {
              return null;
             }
           };
          },function(arg10)
          {
           return EventsPervasives.Events().OnClick(arg00,arg10);
          });
          f5(rb);
          Label={
           $:1,
           $0:function()
           {
            var x5,_this;
            x5=List.ofArray([Default.Text(label)]);
            _this=Default.Tags();
            return _this.NewTag("label",x5);
           }
          };
          return Runtime.New(Body,{
           Element:rb,
           Label:Label
          });
         });
        },function(list)
        {
         return List.mapi(mapping2,list);
        }),f6(rbLbVls)),(f7=function(vs)
        {
         return Tree.FromSequence(vs);
        },f7(x4))),(f8=function(arg0)
        {
         return Runtime.New(Edit,{
          $:0,
          $0:arg0
         });
        },f8(x3))),(f9=function(arg00)
        {
         return Data.RX().Return(arg00);
        },f9(x2)));
        return Runtime.New(Form,{
         Body:body,
         Dispose1:function(value)
         {
          value;
         },
         Notify:function()
         {
          return reset(null);
         },
         State:state
        });
       });
      },
      ReadOnlyCheckbox:function(def)
      {
       return Controls.CheckboxControl(true,def);
      },
      ReadOnlyInput:function(value)
      {
       return Controls.InputField(true,"text",CssConstants.InputTextClass(),value);
      },
      ReadOnlyRadioButtonGroup:function(def,values)
      {
       return Controls.RadioButtonGroupControl(true,def,values);
      },
      ReadOnlySelect:function(def,vls)
      {
       return Controls.SelectControl(true,def,vls);
      },
      ReadOnlyTextArea:function(value)
      {
       return Controls.TextAreaControl(true,value);
      },
      Select:function(def,vls)
      {
       return Controls.SelectControl(false,def,vls);
      },
      SelectControl:function(readOnly,def,vls)
      {
       return Data.MkFormlet(function()
       {
        var aVls,x,f,mapping,f1,sIx,body,select,x1,f2,mapping1,f3,_this2,sValue,state,reset,f4,arg001;
        aVls=(x=(f=(mapping=Runtime.Tupled(function(tuple)
        {
         return tuple[1];
        }),function(list)
        {
         return List.map(mapping,list);
        }),f(vls)),(f1=function(list)
        {
         return Arrays.ofSeq(list);
        },f1(x)));
        sIx=(def>=0?def<vls.get_Length():false)?def:0;
        body=(select=(x1=(f2=(mapping1=function(i)
        {
         return Runtime.Tupled(function(tupledArg)
         {
          var nm,_arg1,_this,x2,_this1,x3;
          nm=tupledArg[0];
          _arg1=tupledArg[1];
          _this=Default.Tags();
          x2=List.ofArray([Default.Text(nm),(_this1=Default.Attr(),(x3=Global.String(i),_this1.NewAttr("value",x3)))]);
          return _this.NewTag("option",x2);
         });
        },function(list)
        {
         return List.mapi(mapping1,list);
        }),f2(vls)),(f3=function(x2)
        {
         return Default.Select(x2);
        },f3(x1))),readOnly?Operators.add(select,List.ofArray([(_this2=Default.Attr(),_this2.NewAttr("disabled","disabled"))])):select);
        sValue=Runtime.New(Result,{
         $:0,
         $0:aVls[sIx]
        });
        state=HotStream.New(sValue);
        reset=function()
        {
         var value,objectArg,arg00;
         value=Global.String(sIx);
         objectArg=body["HtmlProvider@32"];
         ((arg00=body.Body,function(arg10)
         {
          return function(arg20)
          {
           return objectArg.SetProperty(arg00,arg10,arg20);
          };
         })("value"))(value);
         return state.Trigger(sValue);
        };
        reset(null);
        f4=(arg001=function()
        {
         var value,x2,x3,f5,f6;
         if(!readOnly)
          {
           value=body.get_Value();
           x2=(x3=aVls[value<<0],(f5=function(arg0)
           {
            return Runtime.New(Result,{
             $:0,
             $0:arg0
            });
           },f5(x3)));
           f6=function(arg00)
           {
            return state.Trigger(arg00);
           };
           return f6(x2);
          }
         else
          {
           return null;
          }
        },function(arg10)
        {
         return EventsPervasives.Events().OnChange(arg001,arg10);
        });
        f4(body);
        reset(null);
        return[body,reset,state];
       });
      },
      TextArea:function(value)
      {
       return Controls.TextAreaControl(false,value);
      },
      TextAreaControl:function(readOnly,value)
      {
       return Controls.InputControl(value,function(state)
       {
        var input,x,_this,f;
        input=(x=readOnly?List.ofArray([(_this=Default.Attr(),_this.NewAttr("readonly","readonly"))]):Runtime.New(T,{
         $:0
        }),Default.TextArea(x));
        f=function(control)
        {
         return Controls.OnTextChange(function()
         {
          if(!readOnly)
           {
            return state.Trigger(Runtime.New(Result,{
             $:0,
             $0:input.get_Value()
            }));
           }
          else
           {
            return null;
           }
         },control);
        };
        f(input);
        return input;
       });
      }
     },
     CssConstants:{
      InputTextClass:Runtime.Field(function()
      {
       return"inputText";
      })
     },
     Data:{
      $:function(f,x)
      {
       var formlet,objectArg;
       formlet=(objectArg=Data.BaseFormlet(),objectArg.Apply(f,x));
       return Data.OfIFormlet(formlet);
      },
      BaseFormlet:function()
      {
       return FormletProvider.New(Data.UtilsProvider());
      },
      DefaultLayout:Runtime.Field(function()
      {
       return Data.Layout().get_Vertical();
      }),
      Formlet:Runtime.Class({
       Build:function()
       {
        return this.BuildInternal.call(null,null);
       },
       MapResult:function(f)
       {
        var x,_this=this;
        x=Runtime.New(Formlet3,{
         BuildInternal:function()
         {
          var form,objectArg,arg00;
          form=_this.BuildInternal.call(null,null);
          return Runtime.New(Form,{
           Body:form.Body,
           Dispose1:form.Dispose1,
           Notify:form.Notify,
           State:(objectArg=_this.Utils.Reactive,(arg00=form.State,function(arg10)
           {
            return objectArg.Select(arg00,arg10);
           })(f))
          });
         },
         LayoutInternal:_this.LayoutInternal,
         ElementInternal:{
          $:0
         },
         FormletBase:_this.FormletBase,
         Utils:_this.Utils
        });
        return x;
       },
       Render:function()
       {
        return this.Run(function(value)
        {
         value;
        }).Render();
       },
       Run:function(f)
       {
        var matchValue,formlet,form,value,el,matchValue1,patternInput,body,body1,el1;
        matchValue=this.ElementInternal;
        if(matchValue.$==0)
         {
          formlet=this.FormletBase.ApplyLayout(this);
          form=formlet.Build();
          value=Util.subscribeTo(form.State,function(res)
          {
           var value1;
           value1=Result.Map(f,res);
           value1;
          });
          value;
          el=(matchValue1=formlet.get_Layout().Apply.call(null,form.Body),matchValue1.$==0?(patternInput=Data.DefaultLayout().Apply.call(null,form.Body).$0,(body=patternInput[0],body.Element)):(body1=matchValue1.$0[0],body1.Element));
          this.ElementInternal={
           $:1,
           $0:el
          };
          return el;
         }
        else
         {
          el1=matchValue.$0;
          return el1;
         }
       },
       get_Body:function()
       {
        return this.Run(function(value)
        {
         value;
        }).get_Body();
       },
       get_Layout:function()
       {
        return this.LayoutInternal;
       }
      }),
      Layout:Runtime.Field(function()
      {
       return LayoutProvider.New(LayoutUtils.New({
        Reactive:Reactive1.Default()
       }));
      }),
      MkFormlet:function(f)
      {
       var formlet,objectArg;
       formlet=(objectArg=Data.BaseFormlet(),objectArg.New(function()
       {
        var patternInput,state,reset,body,Notify,x,x1,f1,f2;
        patternInput=f(null);
        state=patternInput[2];
        reset=patternInput[1];
        body=patternInput[0];
        Notify=function()
        {
         return reset(null);
        };
        return Runtime.New(Form,{
         Body:(x=(x1=Data.NewBody(body,{
          $:0
         }),(f1=function(value)
         {
          return Tree.Set(value);
         },f1(x1))),(f2=function(arg00)
         {
          return Data.RX().Return(arg00);
         },f2(x))),
         Dispose1:function()
         {
          return null;
         },
         Notify:Notify,
         State:state
        });
       }));
       return Data.OfIFormlet(formlet);
      },
      NewBody:function(arg00,arg10)
      {
       return Body.New(arg00,arg10);
      },
      OfIFormlet:function(formlet)
      {
       var f2;
       f2=Runtime.New(Formlet3,{
        BuildInternal:function()
        {
         return formlet.Build();
        },
        LayoutInternal:formlet.get_Layout(),
        ElementInternal:{
         $:0
        },
        FormletBase:Data.BaseFormlet(),
        Utils:Data.UtilsProvider()
       });
       return Data.PropagateRenderFrom(formlet,f2);
      },
      PropagateRenderFrom:function(f1,f2)
      {
       if(f1.hasOwnProperty("Render"))
        {
         f2.Render=f1.Render;
        }
       return f2;
      },
      RX:Runtime.Field(function()
      {
       return Reactive1.Default();
      }),
      UtilsProvider:function()
      {
       return{
        Reactive:Data.RX(),
        DefaultLayout:Data.DefaultLayout()
       };
      },
      Validator:Runtime.Field(function()
      {
       return Validator.New(ValidatorProvidor.New());
      }),
      ValidatorProvidor:Runtime.Class({
       Matches:function(regex,text)
       {
        return text.match(new RegExp(regex));
       }
      },{
       New:function()
       {
        var r;
        r=Runtime.New(this,{});
        return r;
       }
      })
     },
     ElementStore:Runtime.Class({
      Init:function()
      {
       this.store=Dictionary.New21();
      },
      RegisterElement:function(key,f)
      {
       var value;
       if(value=this.store.ContainsKey(key),!value)
        {
         return this.store.set_Item(key,f);
        }
       else
        {
         return null;
        }
      },
      Remove:function(key)
      {
       var value;
       if(this.store.ContainsKey(key))
        {
         (this.store.get_Item(key))(null);
         value=this.store.Remove(key);
         value;
        }
       else
        {
         return null;
        }
      }
     },{
      New:function()
      {
       var r;
       r=Runtime.New(this,{});
       return r;
      },
      NewElementStore:function()
      {
       var store;
       store=ElementStore.New();
       store.Init();
       return store;
      }
     }),
     Enhance:{
      Cancel:function(formlet,isCancel)
      {
       return Formlet2.Replace(formlet,function(value)
       {
        if(isCancel(value))
         {
          return Formlet2.Empty();
         }
        else
         {
          return Formlet2.Return(value);
         }
       });
      },
      CustomMany:function(config,formlet)
      {
       var addButton,x,f,delF,formlet2,x1,c,x2,f1,f2,l,resetS,resetF,x3,f3,reset,x4,_builder_,f8;
       addButton=(x=Controls.ElementButton(function()
       {
        return Operators.add(Default.Div(List.ofArray([Default.Attr().Class(config.AddIconClass)])),List.ofArray([Default.Div(Runtime.New(T,{
         $:0
        }))]));
       }),(f=function(formlet1)
       {
        return Formlet2.InitWith(1,formlet1);
       },f(x)));
       delF=(formlet2=(x1=(c=(x2=Controls.ElementButton(function()
       {
        return Operators.add(Default.Div(List.ofArray([Default.Attr().Class(config.RemoveIconClass)])),List.ofArray([Default.Div(Runtime.New(T,{
         $:0
        }))]));
       }),(f1=function(formlet1)
       {
        return Formlet2.Map(function(value)
        {
         value;
        },formlet1);
       },f1(x2))),Formlet2.WithCancelation(formlet,c)),(f2=(l=Data.Layout().get_Horizontal(),function(formlet1)
       {
        return Formlet2.WithLayout(l,formlet1);
       }),f2(x1))),Enhance.Deletable(formlet2));
       resetS=HotStream.New(Runtime.New(Result,{
        $:0,
        $0:null
       }));
       resetF=(x3=Data.BaseFormlet().FromState(resetS),(f3=function(formlet1)
       {
        return Data.OfIFormlet(formlet1);
       },f3(x3)));
       reset=function()
       {
        return resetS.Trigger(Runtime.New(Result,{
         $:0,
         $0:null
        }));
       };
       x4=(_builder_=Formlet2.Do(),_builder_.Delay(function()
       {
        return _builder_.Bind(resetF,function()
        {
         return _builder_.ReturnFrom(function()
         {
          var x5,x6,x7,f4,f5,f6,f7;
          x5=(x6=(x7=Enhance.Many_(addButton,function()
          {
           return delF;
          }),(f4=(f5=function(source)
          {
           return List.ofSeq(source);
          },function(formlet1)
          {
           return Formlet2.Map(f5,formlet1);
          }),f4(x7))),(f6=function(formlet1)
          {
           return Formlet2.WithLayoutOrDefault(formlet1);
          },f6(x6)));
          f7=function(formlet1)
          {
           return Formlet2.ApplyLayout(formlet1);
          };
          return f7(x5);
         }(null));
        });
       }));
       f8=function(formlet1)
       {
        return Formlet2.WithNotification(reset,formlet1);
       };
       return f8(x4);
      },
      Deletable:function(formlet)
      {
       return Enhance.Replace(formlet,function(value)
       {
        var value1;
        if(value.$==1)
         {
          value1=value.$0;
          return Formlet2.Return({
           $:1,
           $0:value1
          });
         }
        else
         {
          return Formlet2.ReturnEmpty({
           $:0
          });
         }
       });
      },
      FormButtonConfiguration:Runtime.Class({},{
       get_Default:function()
       {
        return Runtime.New(FormButtonConfiguration,{
         Label:{
          $:0
         },
         Style:{
          $:0
         },
         Class:{
          $:0
         }
        });
       }
      }),
      FormContainerConfiguration:Runtime.Class({},{
       get_Default:function()
       {
        var Description;
        Description={
         $:0
        };
        return Runtime.New(FormContainerConfiguration,{
         Header:{
          $:0
         },
         Padding:Padding.get_Default(),
         Description:Description,
         BackgroundColor:{
          $:0
         },
         BorderColor:{
          $:0
         },
         CssClass:{
          $:0
         },
         Style:{
          $:0
         }
        });
       }
      }),
      InputButton:function(conf,enabled)
      {
       return Data.MkFormlet(function()
       {
        var state,count,submit,label,submit1,x1,_this,_this1,f,arg00,objectArg,arg001,matchValue,style,objectArg1,arg002,matchValue1,cls,objectArg2,arg003,reset;
        state=HotStream.New(Runtime.New(Result,{
         $:1,
         $0:Runtime.New(T,{
          $:0
         })
        }));
        count={
         contents:0
        };
        submit=(label=Utils.Maybe("Submit",function(x)
        {
         return x;
        },conf.Label),(submit1=(x1=Operators.add(Default.Input(List.ofArray([(_this=Default.Attr(),_this.NewAttr("type","button"))])),List.ofArray([Default.Attr().Class("submitButton"),(_this1=Default.Attr(),_this1.NewAttr("value",label))])),(f=(arg00=function()
        {
         return function()
         {
          Operators1.Increment(count);
          return state.Trigger(Runtime.New(Result,{
           $:0,
           $0:count.contents
          }));
         };
        },function(arg10)
        {
         return EventsPervasives.Events().OnClick(arg00,arg10);
        }),(f(x1),x1))),(!enabled?(objectArg=submit1["HtmlProvider@32"],(arg001=submit1.Body,function(arg10)
        {
         return objectArg.AddClass(arg001,arg10);
        })("disabledButton")):null,(matchValue=conf.Style,matchValue.$==1?(style=matchValue.$0,(objectArg1=submit1["HtmlProvider@32"],(arg002=submit1.Body,function(arg10)
        {
         return objectArg1.SetStyle(arg002,arg10);
        })(style))):null,(matchValue1=conf.Class,matchValue1.$==1?(cls=matchValue1.$0,(objectArg2=submit1["HtmlProvider@32"],(arg003=submit1.Body,function(arg10)
        {
         return objectArg2.AddClass(arg003,arg10);
        })(cls))):null,submit1)))));
        reset=function()
        {
         count.contents=0;
         return state.Trigger(Runtime.New(Result,{
          $:1,
          $0:Runtime.New(T,{
           $:0
          })
         }));
        };
        state.Trigger(Runtime.New(Result,{
         $:1,
         $0:Runtime.New(T,{
          $:0
         })
        }));
        return[submit,reset,state];
       });
      },
      Many:function(formlet)
      {
       return Enhance.CustomMany(ManyConfiguration.get_Default(),formlet);
      },
      ManyConfiguration:Runtime.Class({},{
       get_Default:function()
       {
        return Runtime.New(ManyConfiguration,{
         AddIconClass:"addIcon",
         RemoveIconClass:"removeIcon"
        });
       }
      }),
      Many_:function(add,f)
      {
       var x,formlet,formlet1,f1,f2,f3,f4;
       x=(formlet=(formlet1=(f1=(f2=f,function(formlet2)
       {
        return Formlet2.Map(f2,formlet2);
       }),f1(add)),Formlet2.SelectMany(formlet1)),Formlet2.FlipBody(formlet));
       f3=(f4=function(source)
       {
        return Seq.choose(function(x1)
        {
         return x1;
        },source);
       },function(formlet2)
       {
        return Formlet2.Map(f4,formlet2);
       });
       return f3(x);
      },
      Padding:Runtime.Class({},{
       get_Default:function()
       {
        return Runtime.New(Padding,{
         Left:{
          $:0
         },
         Right:{
          $:0
         },
         Top:{
          $:0
         },
         Bottom:{
          $:0
         }
        });
       }
      }),
      Replace:function(formlet,f)
      {
       var f1,f2;
       return Formlet2.Switch((f1=(f2=function(res)
       {
        var fs,arg0,s;
        if(res.$==1)
         {
          fs=res.$0;
          arg0=Formlet2.FailWith(fs);
          return Runtime.New(Result,{
           $:0,
           $0:arg0
          });
         }
        else
         {
          s=res.$0;
          return Runtime.New(Result,{
           $:0,
           $0:f(s)
          });
         }
       },function(formlet1)
       {
        return Formlet2.MapResult(f2,formlet1);
       }),f1(formlet)));
      },
      ValidationFrameConfiguration:Runtime.Class({},{
       get_Default:function()
       {
        return Runtime.New(ValidationFrameConfiguration,{
         ValidClass:{
          $:1,
          $0:"successFormlet"
         },
         ValidStyle:{
          $:0
         },
         ErrorClass:{
          $:1,
          $0:"errorFormlet"
         },
         ErrorStyle:{
          $:0
         }
        });
       }
      }),
      ValidationIconConfiguration:Runtime.Class({},{
       get_Default:function()
       {
        return Runtime.New(ValidationIconConfiguration,{
         ValidIconClass:"validIcon",
         ErrorIconClass:"errorIcon"
        });
       }
      }),
      WithCssClass:function(css,formlet)
      {
       var f,f1;
       f=(f1=function(el)
       {
        var objectArg,arg00;
        objectArg=el["HtmlProvider@32"];
        (arg00=el.Body,function(arg10)
        {
         return objectArg.AddClass(arg00,arg10);
        })(css);
        return el;
       },function(formlet1)
       {
        return Formlet2.MapElement(f1,formlet1);
       });
       return f(formlet);
      },
      WithCustomFormContainer:function(fc,formlet)
      {
       var x,f,f1;
       x=(f=function(formlet1)
       {
        return Formlet2.ApplyLayout(formlet1);
       },f(formlet));
       f1=function(formlet1)
       {
        return Formlet2.MapElement(function(formEl)
        {
         var description,x1,f2,d,f3,tb,x2,f4,d1,f5,cell,x3,f6,f7,x4,x5,f8,f9,x6,fa,fb,x7,fd,fe,x8,ff,f10,x9,f11,f12,f13,action,matchValue,style,objectArg1,arg002,matchValue1,cls,objectArg2,arg003;
         description=(x1=fc.Description,(f2=(d=Runtime.New(T,{
          $:0
         }),(f3=function(descr)
         {
          var genEl,text;
          if(descr.$==1)
           {
            genEl=descr.$0;
            return List.ofArray([genEl(null)]);
           }
          else
           {
            text=descr.$0;
            return List.ofArray([Default.P(List.ofArray([Default.Tags().text(text)]))]);
           }
         },function(o)
         {
          return Utils.Maybe(d,f3,o);
         })),f2(x1)));
         tb=(x2=fc.Header,(f4=(d1=Utils.InTable(List.ofArray([List.ofArray([Operators.add(Default.Div(List.ofArray([Default.Attr().Class("headerPanel")])),description)]),List.ofArray([formEl])])),(f5=function(formElem)
         {
          var header,hdr,genElem,text;
          header=(hdr=formElem.$==1?(genElem=formElem.$0,genElem(null)):(text=formElem.$0,Default.H1(List.ofArray([Default.Tags().text(text)]))),Operators.add(Default.Div(List.ofArray([Default.Attr().Class("headerPanel")])),Runtime.New(T,{
           $:1,
           $0:hdr,
           $1:description
          })));
          return Utils.InTable(List.ofArray([List.ofArray([header]),List.ofArray([formEl])]));
         },function(o)
         {
          return Utils.Maybe(d1,f5,o);
         })),f4(x2)));
         cell=Operators.add(Default.TD(List.ofArray([Default.Attr().Class("formlet")])),List.ofArray([tb]));
         x3=fc.BorderColor;
         f6=(f7=function(color)
         {
          var arg00,objectArg,arg001;
          arg00="border-color: "+color;
          objectArg=cell["HtmlProvider@32"];
          return(arg001=cell.Body,function(arg10)
          {
           return objectArg.SetStyle(arg001,arg10);
          })(arg00);
         },function(o)
         {
          return Utils.Maybe(null,f7,o);
         });
         f6(x3);
         x4=List.ofArray([["background-color",(x5=fc.BackgroundColor,(f8=(f9=function(color)
         {
          return color;
         },function(value)
         {
          return Utils.MapOption(f9,value);
         }),f8(x5)))],["padding-left",(x6=fc.Padding.Left,(fa=(fb=function(v)
         {
          return Global.String(v)+"px";
         },function(value)
         {
          return Utils.MapOption(fb,value);
         }),fa(x6)))],["padding-right",(x7=fc.Padding.Right,(fd=(fe=function(v)
         {
          return Global.String(v)+"px";
         },function(value)
         {
          return Utils.MapOption(fe,value);
         }),fd(x7)))],["padding-top",(x8=fc.Padding.Top,(ff=(f10=function(v)
         {
          return Global.String(v)+"px";
         },function(value)
         {
          return Utils.MapOption(f10,value);
         }),ff(x8)))],["padding-bottom",(x9=fc.Padding.Bottom,(f11=(f12=function(v)
         {
          return Global.String(v)+"px";
         },function(value)
         {
          return Utils.MapOption(f12,value);
         }),f11(x9)))]]);
         f13=(action=Runtime.Tupled(function(tupledArg)
         {
          var name,value,v,objectArg,arg00;
          name=tupledArg[0];
          value=tupledArg[1];
          if(value.$==0)
           {
            return null;
           }
          else
           {
            v=value.$0;
            objectArg=cell["HtmlProvider@32"];
            return((arg00=cell.Body,function(arg10)
            {
             return function(arg20)
             {
              return objectArg.SetCss(arg00,arg10,arg20);
             };
            })(name))(v);
           }
         }),function(list)
         {
          return Seq.iter(action,list);
         });
         f13(x4);
         matchValue=fc.Style;
         if(matchValue.$==0)
          {
          }
         else
          {
           style=matchValue.$0;
           objectArg1=cell["HtmlProvider@32"];
           (arg002=cell.Body,function(arg10)
           {
            return objectArg1.SetStyle(arg002,arg10);
           })(style);
          }
         matchValue1=fc.CssClass;
         if(matchValue1.$==0)
          {
          }
         else
          {
           cls=matchValue1.$0;
           objectArg2=cell["HtmlProvider@32"];
           (arg003=cell.Body,function(arg10)
           {
            return objectArg2.AddClass(arg003,arg10);
           })(cls);
          }
         return Default.Table(List.ofArray([Default.TBody(List.ofArray([Default.TR(List.ofArray([cell]))]))]));
        },formlet1);
       };
       return f1(x);
      },
      WithCustomResetButton:function(buttonConf,formlet)
      {
       var buttonConf1,matchValue,reset;
       buttonConf1=(matchValue=buttonConf.Label,matchValue.$==0?Runtime.New(FormButtonConfiguration,{
        Label:{
         $:1,
         $0:"Reset"
        },
        Style:buttonConf.Style,
        Class:buttonConf.Class
       }):(matchValue.$0,buttonConf));
       reset=Enhance.InputButton(buttonConf1,true);
       return Enhance.WithResetFormlet(formlet,reset);
      },
      WithCustomSubmitAndResetButtons:function(submitConf,resetConf,formlet)
      {
       return Enhance.WithSubmitAndReset(formlet,function(reset)
       {
        return function(result)
        {
         var submit,fs,x,f,f1,value,x1,f2,f3,reset1,_builder_,x2,f4,l;
         submit=result.$==1?(fs=result.$0,(x=Enhance.InputButton(submitConf,false),(f=(f1=function()
         {
          return Runtime.New(Result,{
           $:1,
           $0:fs
          });
         },function(formlet1)
         {
          return Formlet2.MapResult(f1,formlet1);
         }),f(x)))):(value=result.$0,(x1=Enhance.InputButton(submitConf,true),(f2=(f3=function()
         {
          return value;
         },function(formlet1)
         {
          return Formlet2.Map(f3,formlet1);
         }),f2(x1))));
         reset1=(_builder_=Formlet2.Do(),_builder_.Delay(function()
         {
          return _builder_.Bind(Formlet2.LiftResult(Enhance.InputButton(resetConf,true)),function(_arg1)
          {
           if(_arg1.$==0)
            {
             reset(null);
            }
           return _builder_.Return(null);
          });
         }));
         x2=Data.$(Data.$(Formlet2.Return(function(v)
         {
          return function()
          {
           return v;
          };
         }),submit),reset1);
         f4=(l=Data.Layout().get_Horizontal(),function(formlet1)
         {
          return Formlet2.WithLayout(l,formlet1);
         });
         return f4(x2);
        };
       });
      },
      WithCustomSubmitButton:function(buttonConf,formlet)
      {
       var buttonConf1,matchValue;
       buttonConf1=(matchValue=buttonConf.Label,matchValue.$==0?Runtime.New(FormButtonConfiguration,{
        Label:{
         $:1,
         $0:"Submit"
        },
        Style:buttonConf.Style,
        Class:buttonConf.Class
       }):(matchValue.$0,buttonConf));
       return Enhance.WithSubmitFormlet(formlet,function(res)
       {
        var x,enabled,f;
        x=(enabled=res.$==0?true:false,Enhance.InputButton(buttonConf1,enabled));
        f=function(formlet1)
        {
         return Formlet2.Map(function(value)
         {
          value;
         },formlet1);
        };
        return f(x);
       });
      },
      WithCustomValidationFrame:function(vc,formlet)
      {
       var f,wrapper;
       f=(wrapper=function(state)
       {
        return function(body)
        {
         var x,f1,f2;
         x=Default.Div(List.ofArray([body.Element]));
         f1=(f2=function(panel)
         {
          var x1,f3;
          x1=Util.subscribeTo(state,function(res)
          {
           var msgs,matchValue,cls,objectArg,arg00,matchValue1,cls1,objectArg1,arg001,matchValue2,style,objectArg2,arg002,objectArg3,arg003,matchValue3,cls2,objectArg4,arg004,matchValue4,cls3,objectArg5,arg005,matchValue5,style1,objectArg6,arg006,objectArg7,arg007;
           if(res.$==1)
            {
             msgs=res.$0;
             matchValue=vc.ValidClass;
             if(matchValue.$==1)
              {
               cls=matchValue.$0;
               objectArg=panel["HtmlProvider@32"];
               (arg00=panel.Body,function(arg10)
               {
                return objectArg.RemoveClass(arg00,arg10);
               })(cls);
              }
             matchValue1=vc.ErrorClass;
             if(matchValue1.$==1)
              {
               cls1=matchValue1.$0;
               objectArg1=panel["HtmlProvider@32"];
               (arg001=panel.Body,function(arg10)
               {
                return objectArg1.AddClass(arg001,arg10);
               })(cls1);
              }
             matchValue2=vc.ErrorStyle;
             if(matchValue2.$==1)
              {
               style=matchValue2.$0;
               objectArg2=panel["HtmlProvider@32"];
               return(arg002=panel.Body,function(arg10)
               {
                return objectArg2.SetStyle(arg002,arg10);
               })(style);
              }
             else
              {
               objectArg3=panel["HtmlProvider@32"];
               return(arg003=panel.Body,function(arg10)
               {
                return objectArg3.SetStyle(arg003,arg10);
               })("");
              }
            }
           else
            {
             matchValue3=vc.ErrorClass;
             if(matchValue3.$==1)
              {
               cls2=matchValue3.$0;
               objectArg4=panel["HtmlProvider@32"];
               (arg004=panel.Body,function(arg10)
               {
                return objectArg4.RemoveClass(arg004,arg10);
               })(cls2);
              }
             matchValue4=vc.ValidClass;
             if(matchValue4.$==1)
              {
               cls3=matchValue4.$0;
               objectArg5=panel["HtmlProvider@32"];
               (arg005=panel.Body,function(arg10)
               {
                return objectArg5.AddClass(arg005,arg10);
               })(cls3);
              }
             matchValue5=vc.ValidStyle;
             if(matchValue5.$==1)
              {
               style1=matchValue5.$0;
               objectArg6=panel["HtmlProvider@32"];
               return(arg006=panel.Body,function(arg10)
               {
                return objectArg6.SetStyle(arg006,arg10);
               })(style1);
              }
             else
              {
               objectArg7=panel["HtmlProvider@32"];
               return(arg007=panel.Body,function(arg10)
               {
                return objectArg7.SetStyle(arg007,arg10);
               })("");
              }
            }
          });
          f3=function(value)
          {
           value;
          };
          return f3(x1);
         },function(w)
         {
          return Operators.OnAfterRender(f2,w);
         });
         f1(x);
         return x;
        };
       },function(formlet1)
       {
        return Enhance.WrapFormlet(wrapper,formlet1);
       });
       return f(formlet);
      },
      WithCustomValidationIcon:function(vic,formlet)
      {
       var formlet1,f,x,x1,_builder_,f3,f4,f5,l;
       formlet1=(f=function(formlet2)
       {
        return Formlet2.WithLayoutOrDefault(formlet2);
       },f(formlet));
       x=(x1=(_builder_=Formlet2.Do(),_builder_.Delay(function()
       {
        return _builder_.Bind(Formlet2.LiftResult(formlet1),function(_arg1)
        {
         return _builder_.Bind(function(res)
         {
          var x2,f2;
          x2=function()
          {
           var msgs,title,f1,_this,_this1;
           if(res.$==1)
            {
             msgs=res.$0;
             title=(f1=function(x3)
             {
              return function(y)
              {
               return x3+" "+y;
              };
             },Seq.fold(f1,"",msgs));
             return Operators.add(Default.Div(List.ofArray([Default.Attr().Class(vic.ErrorIconClass),(_this=Default.Attr(),_this.NewAttr("title",title))])),List.ofArray([Default.Div(Runtime.New(T,{
              $:0
             }))]));
            }
           else
            {
             return Operators.add(Default.Div(List.ofArray([Default.Attr().Class(vic.ValidIconClass),(_this1=Default.Attr(),_this1.NewAttr("title",""))])),List.ofArray([Default.Div(Runtime.New(T,{
              $:0
             }))]));
            }
          };
          f2=function(genElem)
          {
           return Formlet2.OfElement(genElem);
          };
          return f2(x2);
         }(_arg1),function()
         {
          return _builder_.Return(_arg1);
         });
        });
       })),(f3=(f4=function(arg00)
       {
        return Result.Join(arg00);
       },function(formlet2)
       {
        return Formlet2.MapResult(f4,formlet2);
       }),f3(x1)));
       f5=(l=Data.Layout().get_Horizontal(),function(formlet2)
       {
        return Formlet2.WithLayout(l,formlet2);
       });
       return f5(x);
      },
      WithErrorFormlet:function(f,formlet)
      {
       var x,_builder_,f1;
       x=(_builder_=Formlet2.Do(),_builder_.Delay(function()
       {
        return _builder_.Bind(Formlet2.LiftResult(formlet),function(_arg1)
        {
         var msgs,_builder_1;
         return _builder_.ReturnFrom(_arg1.$==1?(msgs=_arg1.$0,(_builder_1=Formlet2.Do(),_builder_1.Delay(function()
         {
          return _builder_1.Bind(f(msgs),function()
          {
           return _builder_1.Return(_arg1);
          });
         }))):(_arg1.$0,Formlet2.Return(_arg1)));
        });
       }));
       f1=function(formlet1)
       {
        return Formlet2.MapResult(function(arg00)
        {
         return Result.Join(arg00);
        },formlet1);
       };
       return f1(x);
      },
      WithErrorSummary:function(label,formlet)
      {
       var x,_builder_,f5,f6;
       x=(_builder_=Formlet2.Do(),_builder_.Delay(function()
       {
        return _builder_.Bind(Formlet2.LiftResult(formlet),function(_arg1)
        {
         var fs,x1,f3,f4,s;
         return _builder_.ReturnFrom(_arg1.$==1?(fs=_arg1.$0,(x1=function(fs1)
         {
          return Formlet2.OfElement(function()
          {
           var x2,x3,_this,x4,f,mapping,f2,_this1;
           x2=List.ofArray([(x3=List.ofArray([Default.Text(label)]),(_this=Default.Tags(),_this.NewTag("legend",x3))),(x4=(f=(mapping=function(f1)
           {
            return Default.LI(List.ofArray([Default.Text(f1)]));
           },function(list)
           {
            return List.map(mapping,list);
           }),f(fs1)),(f2=function(x5)
           {
            return Default.UL(x5);
           },f2(x4)))]);
           _this1=Default.Tags();
           return _this1.NewTag("fieldset",x2);
          });
         }(fs),(f3=(f4=function()
         {
          return _arg1;
         },function(formlet1)
         {
          return Formlet2.Map(f4,formlet1);
         }),f3(x1)))):(s=_arg1.$0,Formlet2.Return(_arg1)));
        });
       }));
       f5=(f6=function(arg00)
       {
        return Result.Join(arg00);
       },function(formlet1)
       {
        return Formlet2.MapResult(f6,formlet1);
       });
       return f5(x);
      },
      WithFormContainer:function(formlet)
      {
       return Enhance.WithCustomFormContainer(FormContainerConfiguration.get_Default(),formlet);
      },
      WithJsonPost:function(conf,formlet)
      {
       var postUrl,matchValue,url,_this,enc,matchValue1,enc1,_this1,hiddenField,_this2,x,_this3,_this4,x1,submitButton,_this5,x2,_this6,_this7,form,formAttrs,_this8,_this9,x3,f,formlet1,f2,f3;
       postUrl=(matchValue=conf.PostUrl,matchValue.$==0?Runtime.New(T,{
        $:0
       }):(url=matchValue.$0,List.ofArray([(_this=Default.Attr(),_this.NewAttr("action",url))])));
       enc=(matchValue1=conf.EncodingType,matchValue1.$==0?Runtime.New(T,{
        $:0
       }):(enc1=matchValue1.$0,List.ofArray([(_this1=Default.Attr(),_this1.NewAttr("enctype",enc1))])));
       hiddenField=(_this2=Default.Tags(),(x=List.ofArray([(_this3=Default.Attr(),_this3.NewAttr("type","hidden")),(_this4=Default.Attr(),(x1=conf.ParameterName,_this4.NewAttr("name",x1)))]),_this2.NewTag("input",x)));
       submitButton=(_this5=Default.Tags(),(x2=List.ofArray([(_this6=Default.Attr(),_this6.NewAttr("type","submit")),(_this7=Default.Attr(),_this7.NewAttr("value","Submit"))]),_this5.NewTag("input",x2)));
       form=(formAttrs=List.append(Runtime.New(T,{
        $:1,
        $0:(_this8=Default.Attr(),_this8.NewAttr("method","POST")),
        $1:Runtime.New(T,{
         $:1,
         $0:(_this9=Default.Attr(),_this9.NewAttr("style","display:none")),
         $1:postUrl
        })
       }),enc),(x3=Operators.add(Default.Form(formAttrs),List.ofArray([hiddenField,submitButton])),(f=function(w)
       {
        return Operators.OnAfterRender(function(form1)
        {
         var matchValue2,enc2,x4,f1;
         matchValue2=conf.EncodingType;
         if(matchValue2.$==0)
          {
           return null;
          }
         else
          {
           enc2=matchValue2.$0;
           if(enc2==="multipart/form-data")
            {
             x4=jQuery(form1.Body).attr("encoding","multipart/form-data");
             f1=function(value)
             {
              value;
             };
             return f1(x4);
            }
           else
            {
             return null;
            }
          }
        },w);
       },(f(x3),x3))));
       formlet1=(f2=(f3=function(value)
       {
        var data;
        data=JSON.stringify(value);
        jQuery(hiddenField.Body).val(data);
        return jQuery(submitButton.Body).click();
       },function(formlet2)
       {
        return Formlet2.Map(f3,formlet2);
       }),f2(formlet));
       return Default.Div(List.ofArray([form,formlet1]));
      },
      WithLabel:function(labelGen,formlet)
      {
       return Formlet2.WithLabel({
        $:1,
        $0:labelGen
       },formlet);
      },
      WithLabelAbove:function(formlet)
      {
       var f,f1;
       f=(f1=function(body)
       {
        var el,label,matchValue,l,Label;
        el=(label=(matchValue=body.Label,matchValue.$==0?Default.Span(Runtime.New(T,{
         $:0
        })):(l=matchValue.$0,l(null))),Default.Table(List.ofArray([Default.TBody(List.ofArray([Default.TR(List.ofArray([Default.TD(List.ofArray([label]))])),Default.TR(List.ofArray([Default.TD(List.ofArray([body.Element]))]))]))])));
        Label={
         $:0
        };
        return Runtime.New(Body,{
         Element:el,
         Label:Label
        });
       },function(formlet1)
       {
        return Formlet2.MapBody(f1,formlet1);
       });
       return f(formlet);
      },
      WithLabelAndInfo:function(label,info,formlet)
      {
       return Enhance.WithLabel(function()
       {
        var x,_this,_this1;
        return Utils.InTable(List.ofArray([List.ofArray([(x=List.ofArray([Default.Text(label)]),(_this=Default.Tags(),_this.NewTag("label",x))),Default.Span(List.ofArray([(_this1=Default.Attr(),_this1.NewAttr("title",info)),Default.Attr().Class("infoIcon")]))])]));
       },formlet);
      },
      WithLabelConfiguration:function(lc,formlet)
      {
       var x,f,f1,l;
       x=(f=function(formlet1)
       {
        return Formlet2.ApplyLayout(formlet1);
       },f(formlet));
       f1=(l=Data.Layout().LabelLayout(lc),function(formlet1)
       {
        return Formlet2.WithLayout(l,formlet1);
       });
       return f1(x);
      },
      WithLabelLeft:function(formlet)
      {
       var f,f1;
       f=(f1=function(body)
       {
        var el,label,matchValue,l,Label;
        el=(label=(matchValue=body.Label,matchValue.$==0?Default.Span(Runtime.New(T,{
         $:0
        })):(l=matchValue.$0,l(null))),Default.Table(List.ofArray([Default.TBody(List.ofArray([Default.TR(List.ofArray([Default.TD(List.ofArray([body.Element])),Default.TD(List.ofArray([label]))]))]))])));
        Label={
         $:0
        };
        return Runtime.New(Body,{
         Element:el,
         Label:Label
        });
       },function(formlet1)
       {
        return Formlet2.MapBody(f1,formlet1);
       });
       return f(formlet);
      },
      WithLegend:function(label,formlet)
      {
       var f,f1;
       f=(f1=function(body)
       {
        var element,x,x1,_this,matchValue,label1,_this1;
        element=(x=List.ofArray([(x1=List.ofArray([Default.Tags().text(label)]),(_this=Default.Tags(),_this.NewTag("legend",x1))),(matchValue=body.Label,matchValue.$==0?body.Element:(label1=matchValue.$0,Utils.InTable(List.ofArray([List.ofArray([label1(null),body.Element])]))))]),(_this1=Default.Tags(),_this1.NewTag("fieldset",x)));
        return Runtime.New(Body,{
         Element:element,
         Label:{
          $:0
         }
        });
       },function(formlet1)
       {
        return Formlet2.MapBody(f1,formlet1);
       });
       return f(formlet);
      },
      WithResetAction:function(f,formlet)
      {
       var formlet1,f2,x,f1,l;
       formlet1=(f2=(x=Formlet2.New(function()
       {
        var form,notify;
        form=formlet.Build();
        notify=function(o)
        {
         if(f(null))
          {
           return form.Notify.call(null,o);
          }
         else
          {
           return null;
          }
        };
        return Runtime.New(Form,{
         Body:form.Body,
         Dispose1:form.Dispose1,
         Notify:notify,
         State:form.State
        });
       }),(f1=(l=formlet.get_Layout(),function(formlet2)
       {
        return Formlet2.WithLayout(l,formlet2);
       }),f1(x))),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      WithResetButton:function(formlet)
      {
       return Enhance.WithCustomResetButton(FormButtonConfiguration.get_Default(),formlet);
      },
      WithResetFormlet:function(formlet,reset)
      {
       var formlet1,formlet2,formlet3,formlet4,formlet5,f,button,formlet7,f2,x,_builder_,f1;
       formlet1=(formlet2=(formlet3=(formlet4=(formlet5=(f=function(formlet6)
       {
        return Formlet2.WithLayoutOrDefault(formlet6);
       },f(formlet)),Formlet2.ApplyLayout(formlet5)),Formlet2.InitWithFailure(formlet4)),Formlet2.LiftResult(formlet3)),Formlet2.WithNotificationChannel(formlet2));
       button=Formlet2.LiftResult(reset);
       formlet7=(f2=(x=(_builder_=Formlet2.Do(),_builder_.Delay(function()
       {
        return _builder_.Bind(formlet1,Runtime.Tupled(function(_arg1)
        {
         var v,notify;
         v=_arg1[0];
         notify=_arg1[1];
         return _builder_.Bind(button,function(_arg2)
         {
          if(_arg2.$==0)
           {
            notify(null);
           }
          return _builder_.Return(v);
         });
        }));
       })),(f1=function(formlet6)
       {
        return Formlet2.MapResult(function(arg00)
        {
         return Result.Join(arg00);
        },formlet6);
       },f1(x))),Data.PropagateRenderFrom(formlet1,f2));
       return Data.OfIFormlet(formlet7);
      },
      WithRowConfiguration:function(rc,formlet)
      {
       var x,f,f1,l;
       x=(f=function(formlet1)
       {
        return Formlet2.ApplyLayout(formlet1);
       },f(formlet));
       f1=(l=Data.Layout().RowLayout(rc),function(formlet1)
       {
        return Formlet2.WithLayout(l,formlet1);
       });
       return f1(x);
      },
      WithSubmitAndReset:function(formlet,submReset)
      {
       var formlet1,f2,_builder_;
       formlet1=(f2=(_builder_=Formlet2.Do(),_builder_.Delay(function()
       {
        var formlet2,formlet3,f;
        return _builder_.Bind((formlet2=(formlet3=(f=function(formlet4)
        {
         return Formlet2.InitWithFailure(formlet4);
        },f(formlet)),Formlet2.LiftResult(formlet3)),Formlet2.WithNotificationChannel(formlet2)),Runtime.Tupled(function(_arg1)
        {
         var res,notify;
         res=_arg1[0];
         notify=_arg1[1];
         return _builder_.ReturnFrom((submReset(notify))(res));
        }));
       })),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      WithSubmitAndResetButtons:function(formlet)
      {
       var f,submitConf,inputRecord,resetConf,inputRecord1;
       f=(submitConf=(inputRecord=FormButtonConfiguration.get_Default(),Runtime.New(FormButtonConfiguration,{
        Label:{
         $:1,
         $0:"Submit"
        },
        Style:inputRecord.Style,
        Class:inputRecord.Class
       })),(resetConf=(inputRecord1=FormButtonConfiguration.get_Default(),Runtime.New(FormButtonConfiguration,{
        Label:{
         $:1,
         $0:"Reset"
        },
        Style:inputRecord1.Style,
        Class:inputRecord1.Class
       })),function(formlet1)
       {
        return Enhance.WithCustomSubmitAndResetButtons(submitConf,resetConf,formlet1);
       }));
       return f(formlet);
      },
      WithSubmitButton:function(formlet)
      {
       return Enhance.WithCustomSubmitButton(FormButtonConfiguration.get_Default(),formlet);
      },
      WithSubmitFormlet:function(formlet,submit)
      {
       var formlet1,f2,x,_builder_,f1;
       formlet1=(f2=(x=(_builder_=Formlet2.Do(),_builder_.Delay(function()
       {
        var formlet2,f;
        return _builder_.Bind((formlet2=(f=function(formlet3)
        {
         return Formlet2.InitWithFailure(formlet3);
        },f(formlet)),Formlet2.LiftResult(formlet2)),function(_arg1)
        {
         return _builder_.Bind(submit(_arg1),function()
         {
          return _builder_.Return(_arg1);
         });
        });
       })),(f1=function(formlet2)
       {
        return Formlet2.MapResult(function(arg00)
        {
         return Result.Join(arg00);
        },formlet2);
       },f1(x))),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      WithTextLabel:function(label,formlet)
      {
       return Enhance.WithLabel(function()
       {
        var x,_this;
        x=List.ofArray([Default.Text(label)]);
        _this=Default.Tags();
        return _this.NewTag("label",x);
       },formlet);
      },
      WithValidationFrame:function(formlet)
      {
       return Enhance.WithCustomValidationFrame(ValidationFrameConfiguration.get_Default(),formlet);
      },
      WithValidationIcon:function(formlet)
      {
       return Enhance.WithCustomValidationIcon(ValidationIconConfiguration.get_Default(),formlet);
      },
      WrapFormlet:function(wrapper,formlet)
      {
       return Data.MkFormlet(function()
       {
        var formlet1,f,form,patternInput,disp,body,panel;
        formlet1=(f=function(formlet2)
        {
         return Formlet2.WithLayoutOrDefault(formlet2);
        },f(formlet));
        form=Formlet2.BuildForm(formlet1);
        patternInput=formlet1.get_Layout().Apply.call(null,form.Body).$0;
        disp=patternInput[1];
        body=patternInput[0];
        panel=(wrapper(form.State))(body);
        return[panel,function()
        {
         return form.Notify.call(null,null);
        },form.State];
       });
      }
     },
     Formlet:{
      ApplyLayout:function(formlet)
      {
       var formlet1,f2;
       formlet1=(f2=Data.BaseFormlet().ApplyLayout(formlet),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      Bind:function(fl,f)
      {
       var formlet,f2,objectArg,arg10;
       formlet=(f2=(objectArg=Data.BaseFormlet(),(arg10=function(x)
       {
        var y;
        y=f(x);
        return y;
       },objectArg.Bind(fl,arg10))),Data.PropagateRenderFrom(fl,f2));
       return Data.OfIFormlet(formlet);
      },
      BindWith:function(compose,formlet,f)
      {
       var formlet1,f2,objectArg,arg20;
       formlet1=(f2=(objectArg=Data.BaseFormlet(),(arg20=f,objectArg.BindWith(compose,formlet,arg20))),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      BuildForm:function(f)
      {
       return Data.BaseFormlet().BuildForm(f);
      },
      BuildFormlet:function(f)
      {
       return Data.MkFormlet(f);
      },
      Choose:function(fs)
      {
       var count,x,x1,x2,fs1,f,mapping,f6,f7,f8;
       count={
        contents:0
       };
       x=(x1=(x2=(fs1=(f=(mapping=function(f1)
       {
        var x3,x4,f2,f3,f4,f5;
        x3=(x4=(f2=(f3=function(x5)
        {
         Operators1.Increment(count);
         return[x5,count.contents];
        },function(formlet)
        {
         return Formlet2.Map(f3,formlet);
        }),f2(f1)),(f4=function(formlet)
        {
         return Formlet2.InitWithFailure(formlet);
        },f4(x4)));
        f5=function(formlet)
        {
         return Formlet2.LiftResult(formlet);
        };
        return f5(x3);
       },function(source)
       {
        return Seq.map(mapping,source);
       }),f(fs)),Formlet2.Sequence(fs1)),(f6=function(formlet)
       {
        return Formlet2.Map(function(xs)
        {
         var x3,x4,x5,f1,chooser,f2,projection,f3,f4,chooser1;
         x3=(x4=(x5=(f1=(chooser=function(x6)
         {
          var v;
          if(x6.$==0)
           {
            v=x6.$0;
            return{
             $:1,
             $0:v
            };
           }
          else
           {
            return{
             $:0
            };
           }
         },function(list)
         {
          return List.choose(chooser,list);
         }),f1(xs)),(f2=(projection=Runtime.Tupled(function(tupledArg)
         {
          var _arg1,ix;
          _arg1=tupledArg[0];
          ix=tupledArg[1];
          return ix;
         }),function(list)
         {
          return List.sortBy(projection,list);
         }),f2(x5))),(f3=function(list)
         {
          return List.rev(list);
         },f3(x4)));
         f4=(chooser1=Runtime.Tupled(function(tupledArg)
         {
          var x6,_arg2;
          x6=tupledArg[0];
          _arg2=tupledArg[1];
          return{
           $:1,
           $0:x6
          };
         }),function(list)
         {
          return Seq.tryPick(chooser1,list);
         });
         return f4(x3);
        },formlet);
       },f6(x2))),(f7=function(arg20)
       {
        return Data.Validator().Is(function(x3)
        {
         return x3.$==1;
        },"",arg20);
       },f7(x1)));
       f8=function(formlet)
       {
        return Formlet2.Map(function(x3)
        {
         return x3.$0;
        },formlet);
       };
       return f8(x);
      },
      Delay:function(f)
      {
       var formlet;
       formlet=Data.BaseFormlet().Delay(function()
       {
        return f(null);
       });
       return Data.OfIFormlet(formlet);
      },
      Deletable:function(formlet)
      {
       var formlet1,f2;
       formlet1=(f2=Data.BaseFormlet().Deletable(formlet),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      Do:Runtime.Field(function()
      {
       return FormletBuilder.New();
      }),
      Empty:function()
      {
       var formlet;
       formlet=Data.BaseFormlet().Empty();
       return Data.OfIFormlet(formlet);
      },
      FailWith:function(fs)
      {
       var formlet;
       formlet=Data.BaseFormlet().FailWith(fs);
       return Data.OfIFormlet(formlet);
      },
      FlipBody:function(formlet)
      {
       var formlet1,f2;
       formlet1=(f2=Data.BaseFormlet().FlipBody(formlet),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      Flowlet:function(formlet)
      {
       var formlet1,f2,objectArg,arg00;
       formlet1=(f2=(objectArg=Data.BaseFormlet(),(arg00=Data.Layout().get_Flowlet(),objectArg.WithLayout(arg00,formlet))),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      Horizontal:function(formlet)
      {
       var formlet1,f2,objectArg,arg00;
       formlet1=(f2=(objectArg=Data.BaseFormlet(),(arg00=Data.Layout().get_Horizontal(),objectArg.WithLayout(arg00,formlet))),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      InitWith:function(value,formlet)
      {
       var formlet1,f2,objectArg;
       formlet1=(f2=(objectArg=Data.BaseFormlet(),objectArg.InitWith(value,formlet)),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      InitWithFailure:function(formlet)
      {
       var formlet1,f2;
       formlet1=(f2=Data.BaseFormlet().InitWithFailure(formlet),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      Join:function(formlet)
      {
       var formlet1,f2,x,f,f1,f4,objectArg;
       formlet1=(f2=(x=(f=(f1=function(f3)
       {
        return f3;
       },function(formlet2)
       {
        return Formlet2.Map(f1,formlet2);
       }),f(formlet)),(f4=(objectArg=Data.BaseFormlet(),function(arg00)
       {
        return objectArg.Join(arg00);
       }),f4(x))),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      LiftResult:function(formlet)
      {
       var formlet1,f2;
       formlet1=(f2=Data.BaseFormlet().LiftResult(formlet),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      Map:function(f,formlet)
      {
       var formlet1,f2,objectArg;
       formlet1=(f2=(objectArg=Data.BaseFormlet(),objectArg.Map(f,formlet)),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      MapBody:function(f,formlet)
      {
       var formlet1,f2,objectArg;
       formlet1=(f2=(objectArg=Data.BaseFormlet(),objectArg.MapBody(f,formlet)),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      MapElement:function(f,formlet)
      {
       var formlet1,f2,f1,objectArg,arg00;
       formlet1=(f2=(f1=(objectArg=Data.BaseFormlet(),(arg00=function(b)
       {
        return Runtime.New(Body,{
         Element:f(b.Element),
         Label:b.Label
        });
       },function(arg10)
       {
        return objectArg.MapBody(arg00,arg10);
       })),f1(formlet)),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      MapResult:function(f,formlet)
      {
       var formlet1,f2,objectArg;
       formlet1=(f2=(objectArg=Data.BaseFormlet(),objectArg.MapResult(f,formlet)),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      Never:function()
      {
       var formlet;
       formlet=Data.BaseFormlet().Never();
       return Data.OfIFormlet(formlet);
      },
      New:function(f)
      {
       var formlet;
       formlet=Data.BaseFormlet().New(f);
       return Data.OfIFormlet(formlet);
      },
      OfElement:function(genElem)
      {
       return Data.MkFormlet(function()
       {
        var elem;
        elem=genElem(null);
        return[elem,function(value)
        {
         value;
        },Data.RX().Return(Runtime.New(Result,{
         $:0,
         $0:null
        }))];
       });
      },
      Render:function(formlet)
      {
       var f2;
       f2=formlet.Run(function(value)
       {
        value;
       });
       return Data.PropagateRenderFrom(formlet,f2);
      },
      Replace:function(formlet,f)
      {
       var formlet1,f2,objectArg,arg10;
       formlet1=(f2=(objectArg=Data.BaseFormlet(),(arg10=f,objectArg.Replace(formlet,arg10))),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      ReplaceFirstWithFailure:function(formlet)
      {
       var formlet1,f2;
       formlet1=(f2=Data.BaseFormlet().ReplaceFirstWithFailure(formlet),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      Return:function(x)
      {
       var formlet;
       formlet=Data.BaseFormlet().Return(x);
       return Data.OfIFormlet(formlet);
      },
      ReturnEmpty:function(x)
      {
       var formlet;
       formlet=Data.BaseFormlet().ReturnEmpty(x);
       return Data.OfIFormlet(formlet);
      },
      Run:function(f,formlet)
      {
       return formlet.Run(f);
      },
      SelectMany:function(formlet)
      {
       var formlet1,f2,x,f,f1,f4,objectArg;
       formlet1=(f2=(x=(f=(f1=function(f3)
       {
        return f3;
       },function(formlet2)
       {
        return Formlet2.Map(f1,formlet2);
       }),f(formlet)),(f4=(objectArg=Data.BaseFormlet(),function(arg00)
       {
        return objectArg.SelectMany(arg00);
       }),f4(x))),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      Sequence:function(fs)
      {
       var formlet,x,f,mapping,f1,objectArg;
       formlet=(x=(f=(mapping=function(x1)
       {
        return x1;
       },function(source)
       {
        return Seq.map(mapping,source);
       }),f(fs)),(f1=(objectArg=Data.BaseFormlet(),function(arg00)
       {
        return objectArg.Sequence(arg00);
       }),f1(x)));
       return Data.OfIFormlet(formlet);
      },
      Switch:function(formlet)
      {
       var formlet1,f2,x,f,f1,f4,objectArg;
       formlet1=(f2=(x=(f=(f1=function(f3)
       {
        return f3;
       },function(formlet2)
       {
        return Formlet2.Map(f1,formlet2);
       }),f(formlet)),(f4=(objectArg=Data.BaseFormlet(),function(arg00)
       {
        return objectArg.Switch(arg00);
       }),f4(x))),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      Vertical:function(formlet)
      {
       var formlet1,f2,objectArg,arg00;
       formlet1=(f2=(objectArg=Data.BaseFormlet(),(arg00=Data.Layout().get_Vertical(),objectArg.WithLayout(arg00,formlet))),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      WithCancelation:function(formlet,c)
      {
       var formlet1,f2,objectArg;
       formlet1=(f2=(objectArg=Data.BaseFormlet(),objectArg.WithCancelation(formlet,c)),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      WithLabel:function(label,formlet)
      {
       var formlet1,f2,f,objectArg,arg00;
       formlet1=(f2=(f=(objectArg=Data.BaseFormlet(),(arg00=function(body)
       {
        return Runtime.New(Body,{
         Element:body.Element,
         Label:label
        });
       },function(arg10)
       {
        return objectArg.MapBody(arg00,arg10);
       })),f(formlet)),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      WithLayout:function(l,formlet)
      {
       var formlet1,f2,objectArg;
       formlet1=(f2=(objectArg=Data.BaseFormlet(),objectArg.WithLayout(l,formlet)),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      WithLayoutOrDefault:function(formlet)
      {
       var formlet1,f2;
       formlet1=(f2=Data.BaseFormlet().WithLayoutOrDefault(formlet),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      WithNotification:function(c,formlet)
      {
       var formlet1,f2,objectArg;
       formlet1=(f2=(objectArg=Data.BaseFormlet(),objectArg.WithNotification(c,formlet)),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      WithNotificationChannel:function(formlet)
      {
       var formlet1,f2;
       formlet1=(f2=Data.BaseFormlet().WithNotificationChannel(formlet),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      }
     },
     FormletBuilder:Runtime.Class({
      Bind:function(formlet,f)
      {
       var formlet1,f2,objectArg,arg10;
       formlet1=(f2=(objectArg=Data.BaseFormlet(),(arg10=function(x)
       {
        var y;
        y=f(x);
        return y;
       },objectArg.Bind(formlet,arg10))),Data.PropagateRenderFrom(formlet,f2));
       return Data.OfIFormlet(formlet1);
      },
      Delay:function(f)
      {
       var formlet;
       formlet=Data.BaseFormlet().Delay(f);
       return Data.OfIFormlet(formlet);
      },
      Return:function(x)
      {
       var formlet;
       formlet=Data.BaseFormlet().Return(x);
       return Data.OfIFormlet(formlet);
      },
      ReturnFrom:function(f)
      {
       var f1;
       f1=function(formlet)
       {
        return Data.OfIFormlet(formlet);
       };
       return f1(f);
      }
     },{
      New:function()
      {
       var r;
       r=Runtime.New(this,{});
       return r;
      }
     }),
     Layout:{
      FormRowConfiguration:Runtime.Class({},{
       get_Default:function()
       {
        return Runtime.New(FormRowConfiguration,{
         Padding:{
          $:0
         },
         Color:{
          $:0
         },
         Class:{
          $:0
         },
         Style:{
          $:0
         },
         LabelConfiguration:{
          $:0
         }
        });
       }
      }),
      LabelConfiguration:Runtime.Class({},{
       get_Default:function()
       {
        return Runtime.New(LabelConfiguration,{
         Align:{
          $:0
         },
         VerticalAlign:{
          $:1
         },
         Placement:{
          $:0
         }
        });
       }
      }),
      Padding:Runtime.Class({},{
       get_Default:function()
       {
        return Runtime.New(Padding1,{
         Left:{
          $:0
         },
         Right:{
          $:0
         },
         Top:{
          $:0
         },
         Bottom:{
          $:0
         }
        });
       }
      })
     },
     LayoutProvider:Runtime.Class({
      ColumnLayout:function(rowConfig)
      {
       var objectArg,_this=this;
       objectArg=this.LayoutUtils;
       return objectArg.New(function()
       {
        var row,container,store,insert,remove;
        row=Default.TR(Runtime.New(T,{
         $:0
        }));
        container=Default.Table(List.ofArray([Default.TBody(List.ofArray([row]))]));
        store=ElementStore.NewElementStore();
        insert=function(rowIx)
        {
         return function(body)
         {
          var elemId,newCol,jqPanel,index,inserted;
          elemId=body.Element.get_Id();
          newCol=Default.TD(List.ofArray([Default.Table(List.ofArray([Default.TBody(List.ofArray([function(arg20)
          {
           return _this.MakeRow(rowConfig,rowIx,arg20);
          }(body)]))]))]));
          jqPanel=jQuery(row.Body);
          index={
           contents:0
          };
          inserted={
           contents:false
          };
          jqPanel.children().each(function()
          {
           var jqCol;
           jqCol=jQuery(this);
           if(rowIx===index.contents)
            {
             jQuery(newCol.Body).insertBefore(jqCol);
             newCol.Render();
             inserted.contents=true;
            }
           return Operators1.Increment(index);
          });
          if(!inserted.contents)
           {
            row.AppendI(newCol);
           }
          return store.RegisterElement(elemId,function()
          {
           return newCol["HtmlProvider@32"].Remove(newCol.Body);
          });
         };
        };
        remove=function(elems)
        {
         var enumerator;
         enumerator=Enumerator.Get(elems);
         Runtime.While(function()
         {
          return enumerator.MoveNext();
         },function()
         {
          var b;
          b=enumerator.get_Current(),store.Remove(b.Element.get_Id());
         });
        };
        return{
         Body:Runtime.New(Body,{
          Element:container,
          Label:{
           $:0
          }
         }),
         SyncRoot:null,
         Insert:insert,
         Remove:remove
        };
       });
      },
      HorizontalAlignElem:function(align,el)
      {
       var _float,_this,x;
       _float=align.$==0?"left":"right";
       return Operators.add(Default.Div(List.ofArray([(_this=Default.Attr(),(x="float:"+_float+";",_this.NewAttr("style",x)))])),List.ofArray([el]));
      },
      LabelLayout:function(lc)
      {
       var inputRecord,LabelConfiguration1;
       return this.RowLayout((inputRecord=FormRowConfiguration.get_Default(),(LabelConfiguration1={
        $:1,
        $0:lc
       },Runtime.New(FormRowConfiguration,{
        Padding:inputRecord.Padding,
        Color:inputRecord.Color,
        Class:inputRecord.Class,
        Style:inputRecord.Style,
        LabelConfiguration:LabelConfiguration1
       }))));
      },
      MakeLayout:function(lm)
      {
       var objectArg;
       objectArg=this.LayoutUtils;
       return objectArg.New(function()
       {
        var lm1,store,insert,remove;
        lm1=lm(null);
        store=ElementStore.NewElementStore();
        insert=function(ix)
        {
         return function(bd)
         {
          var elemId,newElems;
          elemId=bd.Element.get_Id();
          newElems=(lm1.Insert.call(null,ix))(bd);
          return store.RegisterElement(elemId,function()
          {
           var enumerator;
           enumerator=Enumerator.Get(newElems);
           Runtime.While(function()
           {
            return enumerator.MoveNext();
           },function()
           {
            var e;
            e=enumerator.get_Current(),e["HtmlProvider@32"].Remove(e.Body);
           });
          });
         };
        };
        remove=function(elems)
        {
         var enumerator;
         enumerator=Enumerator.Get(elems);
         Runtime.While(function()
         {
          return enumerator.MoveNext();
         },function()
         {
          var b;
          b=enumerator.get_Current(),store.Remove(b.Element.get_Id());
         });
        };
        return{
         Body:Runtime.New(Body,{
          Element:lm1.Panel,
          Label:{
           $:0
          }
         }),
         SyncRoot:null,
         Insert:insert,
         Remove:remove
        };
       });
      },
      MakeRow:function(rowConfig,rowIndex,body)
      {
       var padding,x,f,d,paddingLeft,x2,f1,f2,paddingTop,x3,f3,f4,paddingRight,x4,f5,f6,paddingBottom,x5,f7,f8,makeCell,elem1,cells,matchValue,labelGen,labelConf,x9,fd,d1,label,xa,fe,arg00,_this2=this,matchValue1,xb,ff,xc,f10,rowClass,xd,f11,d2,rowColorStyleProp,xe,f12,d3,rowStyleProp,xf,f13,d4,rowStyle,matchValue2,x10,f14,b2,b3;
       padding=(x=rowConfig.Padding,(f=(d=Padding1.get_Default(),function(o)
       {
        return Utils.Maybe(d,function(x1)
        {
         return x1;
        },o);
       }),f(x)));
       paddingLeft=(x2=padding.Left,(f1=(f2=function(x1)
       {
        return x1;
       },function(o)
       {
        return Utils.Maybe(0,f2,o);
       }),f1(x2)));
       paddingTop=(x3=padding.Top,(f3=(f4=function(x1)
       {
        return x1;
       },function(o)
       {
        return Utils.Maybe(0,f4,o);
       }),f3(x3)));
       paddingRight=(x4=padding.Right,(f5=(f6=function(x1)
       {
        return x1;
       },function(o)
       {
        return Utils.Maybe(0,f6,o);
       }),f5(x4)));
       paddingBottom=(x5=padding.Bottom,(f7=(f8=function(x1)
       {
        return x1;
       },function(o)
       {
        return Utils.Maybe(0,f8,o);
       }),f7(x5)));
       makeCell=function(l)
       {
        return function(t)
        {
         return function(r)
         {
          return function(b)
          {
           return function(csp)
           {
            return function(valign)
            {
             return function(elem)
             {
              var paddingStyle,x1,x6,f9,mapping,fa,valignStyle,fb,fc,style,_this,x8,colSpan,_this1,a,b1;
              paddingStyle=(x1=(x6=List.ofArray([["padding-left: ",l],["padding-top: ",t],["padding-right: ",r],["padding-bottom: ",b]]),(f9=(mapping=Runtime.Tupled(function(tupledArg)
              {
               var k,v;
               k=tupledArg[0];
               v=tupledArg[1];
               return k+Global.String(v)+"px;";
              }),function(list)
              {
               return List.map(mapping,list);
              }),f9(x6))),(fa=function(source)
              {
               return Seq.reduce(function(x7)
               {
                return function(y)
                {
                 return x7+y;
                };
               },source);
              },fa(x1)));
              valignStyle=(fb=(fc=function(valign1)
              {
               var value;
               value=valign1.$==1?"middle":valign1.$==2?"bottom":"top";
               return"vertical-align: "+value+";";
              },function(o)
              {
               return Utils.Maybe("",fc,o);
              }),fb(valign));
              style=(_this=Default.Attr(),(x8=paddingStyle+";"+valignStyle,_this.NewAttr("style",x8)));
              colSpan=csp?List.ofArray([(_this1=Default.Attr(),_this1.NewAttr("colspan","2"))]):Runtime.New(T,{
               $:0
              });
              return Default.TD((a=Runtime.New(T,{
               $:1,
               $0:style,
               $1:colSpan
              }),(b1=List.ofArray([elem]),List.append(a,b1))));
             };
            };
           };
          };
         };
        };
       };
       elem1=body.Element;
       cells=(matchValue=body.Label,matchValue.$==1?(labelGen=matchValue.$0,(labelConf=(x9=rowConfig.LabelConfiguration,(fd=(d1=LabelConfiguration.get_Default(),function(o)
       {
        return Utils.Maybe(d1,function(x1)
        {
         return x1;
        },o);
       }),fd(x9))),(label=(xa=labelGen(null),(fe=(arg00=labelConf.Align,function(arg10)
       {
        return _this2.HorizontalAlignElem(arg00,arg10);
       }),fe(xa))),(matchValue1=labelConf.Placement,matchValue1.$==3?List.ofArray([(xb=Utils.InTable(List.ofArray([List.ofArray([elem1]),List.ofArray([label])])),(ff=(((((makeCell(paddingLeft))(paddingTop))(paddingRight))(paddingBottom))(true))({
        $:0
       }),ff(xb)))]):matchValue1.$==0?List.ofArray([((((((makeCell(paddingLeft))(paddingTop))(0))(paddingBottom))(false))({
        $:1,
        $0:labelConf.VerticalAlign
       }))(label),((((((makeCell(0))(paddingTop))(paddingRight))(paddingBottom))(false))({
        $:0
       }))(elem1)]):matchValue1.$==1?List.ofArray([((((((makeCell(paddingLeft))(paddingTop))(0))(paddingBottom))(false))({
        $:1,
        $0:labelConf.VerticalAlign
       }))(elem1),((((((makeCell(0))(paddingTop))(paddingRight))(paddingBottom))(false))({
        $:0
       }))(label)]):List.ofArray([(xc=Utils.InTable(List.ofArray([List.ofArray([label]),List.ofArray([elem1])])),(f10=(((((makeCell(paddingLeft))(paddingTop))(paddingRight))(paddingBottom))(true))({
        $:0
       }),f10(xc)))]))))):List.ofArray([((((((makeCell(paddingLeft))(paddingTop))(paddingRight))(paddingBottom))(true))({
        $:0
       }))(elem1)]));
       rowClass=(xd=rowConfig.Class,(f11=(d2=Runtime.New(T,{
        $:0
       }),function(o)
       {
        return Utils.Maybe(d2,function(classGen)
        {
         var arg001;
         return List.ofArray([(arg001=classGen(rowIndex),Default.Attr().Class(arg001))]);
        },o);
       }),f11(xd)));
       rowColorStyleProp=(xe=rowConfig.Color,(f12=(d3=Runtime.New(T,{
        $:0
       }),function(o)
       {
        return Utils.Maybe(d3,function(colGen)
        {
         var col;
         col=colGen(rowIndex);
         return List.ofArray(["background-color: "+col]);
        },o);
       }),f12(xe)));
       rowStyleProp=(xf=rowConfig.Style,(f13=(d4=Runtime.New(T,{
        $:0
       }),function(o)
       {
        return Utils.Maybe(d4,function(styleGen)
        {
         return List.ofArray([styleGen(rowIndex)]);
        },o);
       }),f13(xf)));
       rowStyle=(matchValue2=List.append(rowColorStyleProp,rowStyleProp),matchValue2.$==0?Runtime.New(T,{
        $:0
       }):List.ofArray([(x10=Seq.reduce(function(x1)
       {
        return function(y)
        {
         return x1+";"+y;
        };
       },matchValue2),(f14=function(arg001)
       {
        var _this;
        _this=Default.Attr();
        return _this.NewAttr("style",arg001);
       },f14(x10)))]));
       return Default.TR((b2=(b3=List.append(rowStyle,cells),List.append(rowStyle,b3)),List.append(rowClass,b2)));
      },
      RowLayout:function(rowConfig)
      {
       var objectArg,_this=this;
       objectArg=this.LayoutUtils;
       return objectArg.New(function()
       {
        var panel,container,store,insert,remove;
        panel=Default.TBody(Runtime.New(T,{
         $:0
        }));
        container=Default.Table(List.ofArray([panel]));
        store=ElementStore.NewElementStore();
        insert=function(rowIx)
        {
         return function(body)
         {
          var elemId,row,jqPanel,index,inserted;
          elemId=body.Element.get_Id();
          row=function(arg20)
          {
           return _this.MakeRow(rowConfig,rowIx,arg20);
          }(body);
          jqPanel=jQuery(panel.Body);
          index={
           contents:0
          };
          inserted={
           contents:false
          };
          jqPanel.children().each(function()
          {
           var jqRow;
           jqRow=jQuery(this);
           if(rowIx===index.contents)
            {
             jQuery(row.Body).insertBefore(jqRow);
             row.Render();
             inserted.contents=true;
            }
           return Operators1.Increment(index);
          });
          if(!inserted.contents)
           {
            panel.AppendI(row);
           }
          return store.RegisterElement(elemId,function()
          {
           return row["HtmlProvider@32"].Remove(row.Body);
          });
         };
        };
        remove=function(elems)
        {
         var enumerator;
         enumerator=Enumerator.Get(elems);
         Runtime.While(function()
         {
          return enumerator.MoveNext();
         },function()
         {
          var b;
          b=enumerator.get_Current(),store.Remove(b.Element.get_Id());
         });
        };
        return{
         Body:Runtime.New(Body,{
          Element:container,
          Label:{
           $:0
          }
         }),
         SyncRoot:null,
         Insert:insert,
         Remove:remove
        };
       });
      },
      VerticalAlignedTD:function(valign,elem)
      {
       var valign1,cell,objectArg,arg00;
       valign1=valign.$==1?"middle":valign.$==2?"bottom":"top";
       cell=Default.TD(List.ofArray([elem]));
       objectArg=cell["HtmlProvider@32"];
       ((arg00=cell.Body,function(arg10)
       {
        return function(arg20)
        {
         return objectArg.SetCss(arg00,arg10,arg20);
        };
       })("vertical-align"))(valign1);
       return cell;
      },
      get_Flowlet:function()
      {
       return this.MakeLayout(function()
       {
        var panel,insert;
        panel=Default.Div(Runtime.New(T,{
         $:0
        }));
        insert=function()
        {
         return function(bd)
         {
          var label,nextScreen;
          label=bd.Label.$==1?bd.Label.$0.call(null,null):Default.Span(Runtime.New(T,{
           $:0
          }));
          nextScreen=Utils.InTable(List.ofArray([List.ofArray([label,Default.Div(List.ofArray([bd.Element]))])]));
          panel["HtmlProvider@32"].Clear(panel.Body);
          panel.AppendI(nextScreen);
          return List.ofArray([nextScreen]);
         };
        };
        return{
         Insert:insert,
         Panel:panel
        };
       });
      },
      get_Horizontal:function()
      {
       return this.ColumnLayout(FormRowConfiguration.get_Default());
      },
      get_Vertical:function()
      {
       return this.RowLayout(FormRowConfiguration.get_Default());
      }
     },{
      New:function(LayoutUtils1)
      {
       var r;
       r=Runtime.New(this,{});
       r.LayoutUtils=LayoutUtils1;
       return r;
      }
     }),
     Utils:{
      InTable:function(rows)
      {
       var rs,f,mapping,tb;
       rs=(f=(mapping=function(cols)
       {
        var xs,f1,mapping1;
        xs=(f1=(mapping1=function(c)
        {
         return Default.TD(List.ofArray([c]));
        },function(list)
        {
         return List.map(mapping1,list);
        }),f1(cols));
        return Default.TR(xs);
       },function(list)
       {
        return List.map(mapping,list);
       }),f(rows));
       tb=Default.TBody(function(value)
       {
        return value;
       }(rs));
       return Default.Table(List.ofArray([tb]));
      },
      MapOption:function(f,value)
      {
       var v;
       if(value.$==1)
        {
         v=value.$0;
         return{
          $:1,
          $0:f(v)
         };
        }
       else
        {
         return{
          $:0
         };
        }
      },
      Maybe:function(d,f,o)
      {
       if(o.$==0)
        {
         return d;
        }
       else
        {
         return f(o.$0);
        }
      }
     }
    }
   }
  }
 });
 Runtime.OnInit(function()
 {
  WebSharper=Runtime.Safe(Global.IntelliFactory.WebSharper);
  Formlet=Runtime.Safe(WebSharper.Formlet);
  Body=Runtime.Safe(Formlet.Body);
  Controls=Runtime.Safe(Formlet.Controls);
  Html=Runtime.Safe(WebSharper.Html);
  Default=Runtime.Safe(Html.Default);
  List=Runtime.Safe(WebSharper.List);
  Data=Runtime.Safe(Formlet.Data);
  Reactive=Runtime.Safe(Global.IntelliFactory.Reactive);
  HotStream=Runtime.Safe(Reactive.HotStream);
  Formlet1=Runtime.Safe(Global.IntelliFactory.Formlet);
  Base=Runtime.Safe(Formlet1.Base);
  Result=Runtime.Safe(Base.Result);
  T=Runtime.Safe(List.T);
  Operators=Runtime.Safe(Html.Operators);
  jQuery=Runtime.Safe(Global.jQuery);
  EventsPervasives=Runtime.Safe(Html.EventsPervasives);
  Formlet2=Runtime.Safe(Formlet.Formlet);
  Operators1=Runtime.Safe(WebSharper.Operators);
  CssConstants=Runtime.Safe(Formlet.CssConstants);
  Math=Runtime.Safe(Global.Math);
  Seq=Runtime.Safe(WebSharper.Seq);
  Utils=Runtime.Safe(Formlet.Utils);
  Tree=Runtime.Safe(Base.Tree);
  Edit=Runtime.Safe(Tree.Edit);
  Form=Runtime.Safe(Base.Form);
  Arrays=Runtime.Safe(WebSharper.Arrays);
  FormletProvider=Runtime.Safe(Base.FormletProvider);
  Formlet3=Runtime.Safe(Data.Formlet);
  Util=Runtime.Safe(WebSharper.Util);
  LayoutProvider=Runtime.Safe(Formlet.LayoutProvider);
  LayoutUtils=Runtime.Safe(Base.LayoutUtils);
  Reactive1=Runtime.Safe(Reactive.Reactive);
  Validator=Runtime.Safe(Base.Validator);
  ValidatorProvidor=Runtime.Safe(Data.ValidatorProvidor);
  RegExp=Runtime.Safe(Global.RegExp);
  Collections=Runtime.Safe(WebSharper.Collections);
  Dictionary=Runtime.Safe(Collections.Dictionary);
  ElementStore=Runtime.Safe(Formlet.ElementStore);
  Enhance=Runtime.Safe(Formlet.Enhance);
  FormButtonConfiguration=Runtime.Safe(Enhance.FormButtonConfiguration);
  FormContainerConfiguration=Runtime.Safe(Enhance.FormContainerConfiguration);
  Padding=Runtime.Safe(Enhance.Padding);
  ManyConfiguration=Runtime.Safe(Enhance.ManyConfiguration);
  ValidationFrameConfiguration=Runtime.Safe(Enhance.ValidationFrameConfiguration);
  ValidationIconConfiguration=Runtime.Safe(Enhance.ValidationIconConfiguration);
  JSON=Runtime.Safe(Global.JSON);
  FormletBuilder=Runtime.Safe(Formlet.FormletBuilder);
  Layout=Runtime.Safe(Formlet.Layout);
  FormRowConfiguration=Runtime.Safe(Layout.FormRowConfiguration);
  LabelConfiguration=Runtime.Safe(Layout.LabelConfiguration);
  Padding1=Runtime.Safe(Layout.Padding);
  return Enumerator=Runtime.Safe(WebSharper.Enumerator);
 });
 Runtime.OnLoad(function()
 {
  Formlet2.Do();
  Data.Validator();
  Data.RX();
  Data.Layout();
  Data.DefaultLayout();
  CssConstants.InputTextClass();
 });
}());
