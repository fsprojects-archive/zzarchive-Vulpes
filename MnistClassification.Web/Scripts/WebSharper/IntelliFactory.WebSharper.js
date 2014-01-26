(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,WebSharper,Arrays,Operators,Number,Array,Seq,Unchecked,Enumerator,Char,Util,Concurrency,setTimeout,Date,JavaScript,Scheduler,T,Json,List,T1,Error,Math,Remoting,XhrProvider,JSON,Enumerable,Strings,String,RegExp;
 Runtime.Define(Global,{
  IntelliFactory:{
   WebSharper:{
    Arrays:{
     Find:function(f,arr)
     {
      var matchValue,x;
      matchValue=Arrays.tryFind(f,arr);
      if(matchValue.$==0)
       {
        return Operators.FailWith("KeyNotFoundException");
       }
      else
       {
        x=matchValue.$0;
        return x;
       }
     },
     FindIndex:function(f,arr)
     {
      var matchValue,x;
      matchValue=Arrays.tryFindIndex(f,arr);
      if(matchValue.$==0)
       {
        return Operators.FailWith("KeyNotFoundException");
       }
      else
       {
        x=matchValue.$0;
        return x;
       }
     },
     Pick:function(f,arr)
     {
      var matchValue,x;
      matchValue=Arrays.tryPick(f,arr);
      if(matchValue.$==0)
       {
        return Operators.FailWith("KeyNotFoundException");
       }
      else
       {
        x=matchValue.$0;
        return x;
       }
     },
     average:function(arr)
     {
      return Number(Arrays.sum(arr))/Number(arr.length);
     },
     averageBy:function(f,arr)
     {
      return Number(Arrays.sumBy(f,arr))/Number(arr.length);
     },
     blit:function(arr1,start1,arr2,start2,length)
     {
      Arrays.checkRange(arr1,start1,length);
      Arrays.checkRange(arr2,start2,length);
      Runtime.For(0,length-1,function(i)
      {
       arr2[start2+i]=arr1[start1+i];
      });
     },
     checkLength:function(arr1,arr2)
     {
      if(arr1.length!==arr2.length)
       {
        return Operators.FailWith("Arrays differ in length.");
       }
      else
       {
        return null;
       }
     },
     checkRange:function(arr,start,size)
     {
      if((size<0?true:start<0)?true:arr.length<start+size)
       {
        return Operators.FailWith("Index was outside the bounds of the array.");
       }
      else
       {
        return null;
       }
     },
     choose:function(f,arr)
     {
      var q;
      q=[];
      Runtime.For(0,arr.length-1,function(i)
      {
       var matchValue;
       matchValue=f(arr[i]),matchValue.$==0?null:q.push(matchValue.$0);
      });
      return q;
     },
     collect:function(f,x)
     {
      return Array.prototype.concat.apply([],x.map(function(x1)
      {
       return f(x1);
      }));
     },
     concat:function(xs)
     {
      return Array.prototype.concat.apply([],Arrays.ofSeq(xs));
     },
     create:function(size,value)
     {
      var r;
      r=Array(size);
      Runtime.For(0,size-1,function(i)
      {
       r[i]=value;
      });
      return r;
     },
     exists2:function(f,arr1,arr2)
     {
      Arrays.checkLength(arr1,arr2);
      return Seq.exists2(f,arr1,arr2);
     },
     fill:function(arr,start,length,value)
     {
      Arrays.checkRange(arr,start,length);
      Runtime.For(start,start+length-1,function(i)
      {
       arr[i]=value;
      });
     },
     fold2:function(f,zero,arr1,arr2)
     {
      var accum;
      Arrays.checkLength(arr1,arr2);
      accum=zero;
      Runtime.For(0,arr1.length-1,function(i)
      {
       accum=((f(accum))(arr1[i]))(arr2[i]);
      });
      return accum;
     },
     foldBack2:function(f,arr1,arr2,zero)
     {
      var len,accum;
      Arrays.checkLength(arr1,arr2);
      len=arr1.length;
      accum=zero;
      Runtime.For(1,len,function(i)
      {
       accum=((f(arr1[len-i]))(arr2[len-i]))(accum);
      });
      return accum;
     },
     forall2:function(f,arr1,arr2)
     {
      Arrays.checkLength(arr1,arr2);
      return Seq.forall2(f,arr1,arr2);
     },
     init:function(size,f)
     {
      var r;
      if(size<0)
       {
        Operators.FailWith("Negative size given.");
       }
      r=Array(size);
      Runtime.For(0,size-1,function(i)
      {
       r[i]=f(i);
      });
      return r;
     },
     iter2:function(f,arr1,arr2)
     {
      Arrays.checkLength(arr1,arr2);
      Runtime.For(0,arr1.length-1,function(i)
      {
       (f(arr1[i]))(arr2[i]);
      });
     },
     iteri2:function(f,arr1,arr2)
     {
      Arrays.checkLength(arr1,arr2);
      Runtime.For(0,arr1.length-1,function(i)
      {
       ((f(i))(arr1[i]))(arr2[i]);
      });
     },
     map2:function(f,arr1,arr2)
     {
      var r;
      Arrays.checkLength(arr1,arr2);
      r=Array(arr2.length);
      Runtime.For(0,arr2.length-1,function(i)
      {
       r[i]=(f(arr1[i]))(arr2[i]);
      });
      return r;
     },
     mapi2:function(f,arr1,arr2)
     {
      var res;
      Arrays.checkLength(arr1,arr2);
      res=Array(arr1.length);
      Runtime.For(0,arr1.length-1,function(i)
      {
       res[i]=((f(i))(arr1[i]))(arr2[i]);
      });
      return res;
     },
     max:function(x)
     {
      return x.reduce(function(s,x1)
      {
       return(function(e1)
       {
        return function(e2)
        {
         return Operators.Max(e1,e2);
        };
       }(s))(x1);
      });
     },
     maxBy:function(f,arr)
     {
      return arr.reduce(function(s,x)
      {
       return(function(x1)
       {
        return function(y)
        {
         if(Unchecked.Compare(f(x1),f(y))===1)
          {
           return x1;
          }
         else
          {
           return y;
          }
        };
       }(s))(x);
      });
     },
     min:function(x)
     {
      return x.reduce(function(s,x1)
      {
       return(function(e1)
       {
        return function(e2)
        {
         return Operators.Min(e1,e2);
        };
       }(s))(x1);
      });
     },
     minBy:function(f,arr)
     {
      return arr.reduce(function(s,x)
      {
       return(function(x1)
       {
        return function(y)
        {
         if(Unchecked.Compare(f(x1),f(y))===-1)
          {
           return x1;
          }
         else
          {
           return y;
          }
        };
       }(s))(x);
      });
     },
     nonEmpty:function(arr)
     {
      if(arr.length===0)
       {
        return Operators.FailWith("The input array was empty.");
       }
      else
       {
        return null;
       }
     },
     ofSeq:function(xs)
     {
      var q,_enum;
      q=[];
      _enum=Enumerator.Get(xs);
      Runtime.While(function()
      {
       return _enum.MoveNext();
      },function()
      {
       q.push(_enum.get_Current());
      });
      return q;
     },
     partition:function(f,arr)
     {
      var ret1,ret2;
      ret1=[];
      ret2=[];
      Runtime.For(0,arr.length-1,function(i)
      {
       f(arr[i])?ret1.push(arr[i]):ret2.push(arr[i]);
      });
      return[ret1,ret2];
     },
     permute:function(f,arr)
     {
      var ret;
      ret=[];
      Runtime.For(0,arr.length-1,function(i)
      {
       ret[f(i)]=arr[i];
      });
      return ret;
     },
     reverse:function(array,offset,length)
     {
      var a;
      a=Arrays.sub(array,offset,length).slice().reverse();
      return Arrays.blit(a,0,array,offset,a.length);
     },
     scan:function(f,zero,arr)
     {
      var ret;
      ret=Array(1+arr.length);
      ret[0]=zero;
      Runtime.For(0,arr.length-1,function(i)
      {
       ret[i+1]=(f(ret[i]))(arr[i]);
      });
      return ret;
     },
     scanBack:function(f,arr,zero)
     {
      var len,ret;
      len=arr.length;
      ret=Array(1+len);
      ret[len]=zero;
      Runtime.For(0,len-1,function(i)
      {
       ret[len-i-1]=(f(arr[len-i-1]))(ret[len-i]);
      });
      return ret;
     },
     sort:function(arr)
     {
      return Arrays.sortBy(function(x)
      {
       return x;
      },arr);
     },
     sortBy:function(f,arr)
     {
      return arr.slice().sort(Runtime.Tupled(function(tupledArg)
      {
       var x,y;
       x=tupledArg[0];
       y=tupledArg[1];
       return Operators.Compare(f(x),f(y));
      }));
     },
     sortInPlace:function(arr)
     {
      return Arrays.sortInPlaceBy(function(x)
      {
       return x;
      },arr);
     },
     sortInPlaceBy:function(f,arr)
     {
      return arr.sort(Runtime.Tupled(function(tupledArg)
      {
       var x,y;
       x=tupledArg[0];
       y=tupledArg[1];
       return Operators.Compare(f(x),f(y));
      }));
     },
     sortInPlaceWith:function(comparer,arr)
     {
      return arr.sort(Runtime.Tupled(function(tupledArg)
      {
       var x;
       x=tupledArg[0];
       return(comparer(x))(tupledArg[1]);
      }));
     },
     sortWith:function(comparer,arr)
     {
      return arr.slice().sort(Runtime.Tupled(function(tupledArg)
      {
       var x;
       x=tupledArg[0];
       return(comparer(x))(tupledArg[1]);
      }));
     },
     sub:function(arr,start,length)
     {
      Arrays.checkRange(arr,start,length);
      return arr.slice(start,start+length);
     },
     sum:function($arr)
     {
      var $0=this,$this=this;
      var sum=0;
      for(i=0;i<$arr.length;i++)sum+=$arr[i];
      return sum;
     },
     sumBy:function($f,$arr)
     {
      var $0=this,$this=this;
      var sum=0;
      for(i=0;i<$arr.length;i++)sum+=$f($arr[i]);
      return sum;
     },
     tryFind:function(f,arr)
     {
      var res,i;
      res={
       $:0
      };
      i=0;
      Runtime.While(function()
      {
       if(i<arr.length)
        {
         return res.$==0;
        }
       else
        {
         return false;
        }
      },function()
      {
       f(arr[i])?res={
        $:1,
        $0:arr[i]
       }:null,i=i+1;
      });
      return res;
     },
     tryFindIndex:function(f,arr)
     {
      var res,i;
      res={
       $:0
      };
      i=0;
      Runtime.While(function()
      {
       if(i<arr.length)
        {
         return res.$==0;
        }
       else
        {
         return false;
        }
      },function()
      {
       f(arr[i])?res={
        $:1,
        $0:i
       }:null,i=i+1;
      });
      return res;
     },
     tryPick:function(f,arr)
     {
      var res,i;
      res={
       $:0
      };
      i=0;
      Runtime.While(function()
      {
       if(i<arr.length)
        {
         return res.$==0;
        }
       else
        {
         return false;
        }
      },function()
      {
       var matchValue;
       matchValue=f(arr[i]),matchValue.$==1?res=matchValue:null,i=i+1;
      });
      return res;
     },
     unzip:function(arr)
     {
      var x,y;
      x=[];
      y=[];
      Runtime.For(0,arr.length-1,function(i)
      {
       var patternInput,b,a;
       patternInput=arr[i],(b=patternInput[1],(a=patternInput[0],(x.push(a),y.push(b))));
      });
      return[x,y];
     },
     unzip3:function(arr)
     {
      var x,y,z;
      x=[];
      y=[];
      z=[];
      Runtime.For(0,arr.length-1,function(i)
      {
       var matchValue,c,b,a;
       matchValue=arr[i],(c=matchValue[2],(b=matchValue[1],(a=matchValue[0],(x.push(a),(y.push(b),z.push(c))))));
      });
      return[x,y,z];
     },
     zip:function(arr1,arr2)
     {
      var res;
      Arrays.checkLength(arr1,arr2);
      res=Array(arr1.length);
      Runtime.For(0,arr1.length-1,function(i)
      {
       res[i]=[arr1[i],arr2[i]];
      });
      return res;
     },
     zip3:function(arr1,arr2,arr3)
     {
      var res;
      Arrays.checkLength(arr1,arr2);
      Arrays.checkLength(arr2,arr3);
      res=Array(arr1.length);
      Runtime.For(0,arr1.length-1,function(i)
      {
       res[i]=[arr1[i],arr2[i],arr3[i]];
      });
      return res;
     }
    },
    Char:Runtime.Class({},{
     GetNumericValue:function(c)
     {
      if(c>=48?c<=57:false)
       {
        return Number(c)-Number(48);
       }
      else
       {
        return-1;
       }
     },
     IsControl:function(c)
     {
      if(c>=0?c<=31:false)
       {
        return true;
       }
      else
       {
        if(c>=128)
         {
          return c<=159;
         }
        else
         {
          return false;
         }
       }
     },
     IsDigit:function(c)
     {
      if(c>=48)
       {
        return c<=57;
       }
      else
       {
        return false;
       }
     },
     IsLetter:function(c)
     {
      if(c>=65?c<=90:false)
       {
        return true;
       }
      else
       {
        if(c>=97)
         {
          return c<=122;
         }
        else
         {
          return false;
         }
       }
     },
     IsLetterOrDigit:function(c)
     {
      if(Char.IsLetter(c))
       {
        return true;
       }
      else
       {
        return Char.IsDigit(c);
       }
     },
     IsLower:function(c)
     {
      if(c>=97)
       {
        return c<=122;
       }
      else
       {
        return false;
       }
     },
     IsUpper:function(c)
     {
      if(c>=65)
       {
        return c<=90;
       }
      else
       {
        return false;
       }
     },
     IsWhiteSpace:function($c)
     {
      var $0=this,$this=this;
      return Global.String.fromCharCode($c).match(/\s/)!==null;
     }
    }),
    Concurrency:{
     AwaitEvent:function(e)
     {
      return{
       $:0,
       $0:function(k)
       {
        var sub;
        sub={
         contents:undefined
        };
        sub.contents=Util.subscribeTo(e,function(x)
        {
         sub.contents.Dispose();
         return k({
          $:0,
          $0:x
         });
        });
       }
      };
     },
     Bind:function(_arg1,f)
     {
      var r;
      r=_arg1.$0;
      return{
       $:0,
       $0:function(k)
       {
        return r(function(_arg2)
        {
         var e,x;
         if(_arg2.$==1)
          {
           e=_arg2.$0;
           return k({
            $:1,
            $0:e
           });
          }
         else
          {
           x=_arg2.$0;
           return Concurrency.fork(function()
           {
            var e1;
            try
            {
             return Concurrency.Run(f(x),k);
            }
            catch(e1)
            {
             return k({
              $:1,
              $0:e1
             });
            }
           });
          }
        });
       }
      };
     },
     Catch:function(_arg1)
     {
      var r;
      r=_arg1.$0;
      return{
       $:0,
       $0:function(k)
       {
        var e1;
        try
        {
         return r(function(_arg2)
         {
          var e,x;
          if(_arg2.$==1)
           {
            e=_arg2.$0;
            return k({
             $:0,
             $0:{
              $:1,
              $0:e
             }
            });
           }
          else
           {
            x=_arg2.$0;
            return k({
             $:0,
             $0:{
              $:0,
              $0:x
             }
            });
           }
         });
        }
        catch(e1)
        {
         return k({
          $:0,
          $0:{
           $:1,
           $0:e1
          }
         });
        }
       }
      };
     },
     Delay:function(mk)
     {
      return{
       $:0,
       $0:function(k)
       {
        var e;
        try
        {
         return Concurrency.Run(mk(null),k);
        }
        catch(e)
        {
         return k({
          $:1,
          $0:e
         });
        }
       }
      };
     },
     For:function(s,b)
     {
      var ie;
      ie=Enumerator.Get(s);
      return Concurrency.While(function()
      {
       return ie.MoveNext();
      },Concurrency.Delay(function()
      {
       return b(ie.get_Current());
      }));
     },
     FromContinuations:function(subscribe)
     {
      return{
       $:0,
       $0:function(k)
       {
        return(subscribe(function(a)
        {
         return k({
          $:0,
          $0:a
         });
        }))(function(e)
        {
         return k({
          $:1,
          $0:e
         });
        });
       }
      };
     },
     Parallel:function(cs)
     {
      var cs1;
      cs1=Arrays.ofSeq(cs);
      return{
       $:0,
       $0:function(k)
       {
        var n,o,a,f;
        n=cs1.length;
        o={
         contents:n
        };
        a=Arrays.create(n,undefined);
        f=function(i)
        {
         return function(_arg1)
         {
          var run;
          run=_arg1.$0;
          return Concurrency.fork(function()
          {
           return run(function(i1)
           {
            return function(x)
            {
             var matchValue,e,n1,x1,e1,n2,x2,n3;
             matchValue=[o.contents,x];
             if(matchValue[0]===0)
              {
               return null;
              }
             else
              {
               if(matchValue[0]===1)
                {
                 if(matchValue[1].$==1)
                  {
                   e=matchValue[1].$0;
                   n1=matchValue[0];
                   o.contents=0;
                   return k({
                    $:1,
                    $0:e
                   });
                  }
                 else
                  {
                   x1=matchValue[1].$0;
                   a[i1]=x1;
                   o.contents=0;
                   return k({
                    $:0,
                    $0:a
                   });
                  }
                }
               else
                {
                 if(matchValue[1].$==1)
                  {
                   e1=matchValue[1].$0;
                   n2=matchValue[0];
                   o.contents=0;
                   return k({
                    $:1,
                    $0:e1
                   });
                  }
                 else
                  {
                   x2=matchValue[1].$0;
                   n3=matchValue[0];
                   a[i1]=x2;
                   o.contents=n3-1;
                  }
                }
              }
            };
           }(i));
          });
         };
        };
        return cs1.forEach(function(x,i)
        {
         (f(i))(x);
        });
       }
      };
     },
     Return:function(x)
     {
      return{
       $:0,
       $0:function(k)
       {
        return k({
         $:0,
         $0:x
        });
       }
      };
     },
     Run:function(_arg1,x)
     {
      var run;
      run=_arg1.$0;
      return run(x);
     },
     Scheduler:Runtime.Class({
      Fork:function(action)
      {
       var _this=this;
       this.robin.push(action);
       if(this.idle)
        {
         this.idle=false;
         return setTimeout(function()
         {
          return _this.tick();
         },0);
        }
       else
        {
         return null;
        }
      },
      tick:function()
      {
       var t,loop,_this=this;
       t=Date.now();
       loop=true;
       Runtime.While(function()
       {
        return loop;
       },function()
       {
        var matchValue;
        matchValue=_this.robin.length,matchValue===0?(_this.idle=true,loop=false):((_this.robin.shift())(null),Date.now()-t>40?(setTimeout(function()
        {
         return _this.tick();
        },0),loop=false):null);
       });
      }
     },{
      New:function()
      {
       var r;
       r=Runtime.New(this,{});
       r.idle=true;
       r.robin=[];
       return r;
      }
     }),
     Sleep:function(ms)
     {
      return{
       $:0,
       $0:function(k)
       {
        var action;
        action=function()
        {
         return k({
          $:0,
          $0:null
         });
        };
        return setTimeout(action,ms);
       }
      };
     },
     Start:function(c)
     {
      return Concurrency.StartWithContinuations(c,function(value)
      {
       value;
      },function(exn)
      {
       return JavaScript.Log(["WebSharper: Uncaught asynchronous exception",exn]);
      });
     },
     StartChild:function(_arg1)
     {
      var r;
      r=_arg1.$0;
      return{
       $:0,
       $0:function(k)
       {
        var cached,queue;
        cached={
         contents:{
          $:0
         }
        };
        queue=[];
        Concurrency.fork(function()
        {
         return r(function(res)
         {
          cached.contents={
           $:1,
           $0:res
          };
          Runtime.While(function()
          {
           return queue.length>0;
          },function()
          {
           (queue.shift())(res);
          });
         });
        });
        return k({
         $:0,
         $0:{
          $:0,
          $0:function(k1)
          {
           var matchValue,x;
           matchValue=cached.contents;
           if(matchValue.$==0)
            {
             return queue.push(k1);
            }
           else
            {
             x=matchValue.$0;
             return k1(x);
            }
          }
         }
        });
       }
      };
     },
     StartWithContinuations:function(c,s,f)
     {
      return Concurrency.fork(function()
      {
       return Concurrency.Run(c,function(_arg1)
       {
        if(_arg1.$==1)
         {
          return f(_arg1.$0);
         }
        else
         {
          return s(_arg1.$0);
         }
       });
      });
     },
     TryFinally:function(_arg1,f)
     {
      var run;
      run=_arg1.$0;
      return{
       $:0,
       $0:function(k)
       {
        return run(function(r)
        {
         var e;
         try
         {
          f(null);
          return k(r);
         }
         catch(e)
         {
          return k({
           $:1,
           $0:e
          });
         }
        });
       }
      };
     },
     TryWith:function(_arg1,f)
     {
      var r;
      r=_arg1.$0;
      return{
       $:0,
       $0:function(k)
       {
        return r(function(_arg2)
        {
         var e,e1,x;
         if(_arg2.$==1)
          {
           e=_arg2.$0;
           try
           {
            return Concurrency.Run(f(e),k);
           }
           catch(e1)
           {
            return k({
             $:1,
             $0:e1
            });
           }
          }
         else
          {
           x=_arg2.$0;
           return k({
            $:0,
            $0:x
           });
          }
        });
       }
      };
     },
     Using:function(x,f)
     {
      return Concurrency.TryFinally(f(x),function()
      {
       return x.Dispose();
      });
     },
     While:function(g,c)
     {
      if(g(null))
       {
        return Concurrency.Bind(c,function()
        {
         return Concurrency.While(g,c);
        });
       }
      else
       {
        return Concurrency.Return(null);
       }
     },
     fork:function(action)
     {
      return Concurrency.scheduler().Fork(action);
     },
     scheduler:Runtime.Field(function()
     {
      return Scheduler.New();
     })
    },
    DateTimeHelpers:{
     AddMonths:function(d,months)
     {
      var e;
      e=new Date(d);
      return(new Date(e.getFullYear(),e.getMonth()+months,e.getDate(),e.getHours(),e.getMinutes(),e.getSeconds(),e.getMilliseconds())).getTime();
     },
     AddYears:function(d,years)
     {
      var e;
      e=new Date(d);
      return(new Date(e.getFullYear()+years,e.getMonth(),e.getDate(),e.getHours(),e.getMinutes(),e.getSeconds(),e.getMilliseconds())).getTime();
     },
     DatePortion:function(d)
     {
      var e;
      e=new Date(d);
      return(new Date(e.getFullYear(),e.getMonth(),e.getDate())).getTime();
     },
     TimePortion:function(d)
     {
      var e;
      e=new Date(d);
      return(((24*0+e.getHours())*60+e.getMinutes())*60+e.getSeconds())*1000+e.getMilliseconds();
     }
    },
    Enumerable:{
     Of:function(getEnumerator)
     {
      var r;
      r={};
      r.GetEnumerator=getEnumerator;
      return r;
     }
    },
    Enumerator:{
     Get:function(x)
     {
      var next,next1;
      if(x instanceof Global.Array)
       {
        next=function(e)
        {
         var i,v,v1;
         i=e.s;
         if(i<x.length)
          {
           v=x[i];
           e.c=v;
           v1=i+1;
           e.s=v1;
           return true;
          }
         else
          {
           return false;
          }
        };
        return T.New(0,null,next);
       }
      else
       {
        if(Unchecked.Equals(typeof x,"string"))
         {
          next1=function(e)
          {
           var i,v,v1;
           i=e.s;
           if(i<x.length)
            {
             v=x.charCodeAt(i);
             e.c=v;
             v1=i+1;
             e.s=v1;
             return true;
            }
           else
            {
             return false;
            }
          };
          return T.New(0,null,next1);
         }
        else
         {
          return x.GetEnumerator();
         }
       }
     },
     T:Runtime.Class({
      MoveNext:function()
      {
       return this.n.call(null,this);
      },
      get_Current:function()
      {
       return this.c;
      }
     },{
      New:function(s,c,n)
      {
       var r;
       r=Runtime.New(this,{});
       r.s=s;
       r.c=c;
       r.n=n;
       return r;
      }
     })
    },
    JavaScript:{
     Delete:function($x,$field)
     {
      var $0=this,$this=this;
      return delete $x[$field];
     },
     ForEach:function($x,$iter)
     {
      var $0=this,$this=this;
      for(var k in $x){
       if($iter(k))
        break;
      }
     },
     GetFields:function($o)
     {
      var $0=this,$this=this;
      var r=[];
      for(var k in $o)r.push([k,$o[k]]);
      return r;
     },
     Log:function($x)
     {
      var $0=this,$this=this;
      if(Global.console)
       Global.console.log($x);
     }
    },
    Json:{
     Activate:function(json)
     {
      var types,decode;
      types=json.$TYPES;
      Runtime.For(0,types.length-1,function(i)
      {
       types[i]=Json.lookup(types[i]);
      });
      decode=function(x)
      {
       var matchValue,o,ti;
       if(Unchecked.Equals(x,null))
        {
         return x;
        }
       else
        {
         matchValue=typeof x;
         if(matchValue==="object")
          {
           if(x instanceof Global.Array)
            {
             return Json.shallowMap(decode,x);
            }
           else
            {
             o=Json.shallowMap(decode,x.$V);
             ti=x.$T;
             if(Unchecked.Equals(typeof ti,"undefined"))
              {
               return o;
              }
             else
              {
               return Json.restore(types[ti],o);
              }
            }
          }
         else
          {
           return x;
          }
        }
      };
      return decode(json.$DATA);
     },
     lookup:function(x)
     {
      var k,r,i;
      k=x.length;
      r=Global;
      i=0;
      Runtime.While(function()
      {
       return i<k;
      },function()
      {
       var n,rn;
       n=x[i],(rn=r[n],!Unchecked.Equals(typeof rn,undefined)?(r=rn,i=i+1):Operators.FailWith("Invalid server reply. Failed to find type: "+n));
      });
      return r;
     },
     restore:function(ty,obj)
     {
      var r;
      r=new ty();
      JavaScript.ForEach(obj,function(k)
      {
       r[k]=obj[k];
       return false;
      });
      return r;
     },
     shallowMap:function(f,x)
     {
      var matchValue,r;
      if(x instanceof Global.Array)
       {
        return x.map(function(x1)
        {
         return f(x1);
        });
       }
      else
       {
        matchValue=typeof x;
        if(matchValue==="object")
         {
          r={};
          JavaScript.ForEach(x,function(y)
          {
           r[y]=f(x[y]);
           return false;
          });
          return r;
         }
        else
         {
          return x;
         }
       }
     }
    },
    Lazy:{
     Create:function(f)
     {
      var x;
      x={
       value:undefined,
       created:false,
       eval:f
      };
      x.eval=function()
      {
       if(x.created)
        {
         return x.value;
        }
       else
        {
         x.created=true;
         x.value=f(null);
         return x.value;
        }
      };
      return x;
     },
     CreateFromValue:function(v)
     {
      var x;
      x={
       value:v,
       created:true,
       eval:function()
       {
        return v;
       }
      };
      x.eval=function()
      {
       return v;
      };
      return x;
     },
     Force:function(x)
     {
      return x.eval.call(null,null);
     }
    },
    List:{
     T:Runtime.Class({
      GetEnumerator:function()
      {
       var next;
       next=function(e)
       {
        var matchValue,xs,x;
        matchValue=e.s;
        if(matchValue.$==0)
         {
          return false;
         }
        else
         {
          xs=matchValue.$1;
          x=matchValue.$0;
          e.c=x;
          e.s=xs;
          return true;
         }
       };
       return T.New(this,null,next);
      },
      get_Item:function(x)
      {
       return Seq.nth(x,this);
      },
      get_Length:function()
      {
       return Seq.length(this);
      }
     },{
      Construct:function(head,tail)
      {
       return Runtime.New(T1,{
        $:1,
        $0:head,
        $1:tail
       });
      },
      get_Nil:function()
      {
       return Runtime.New(T1,{
        $:0
       });
      }
     }),
     append:function(x,y)
     {
      return List.ofSeq(Seq.append(x,y));
     },
     choose:function(f,l)
     {
      return List.ofSeq(Seq.choose(f,l));
     },
     collect:function(f,l)
     {
      return List.ofSeq(Seq.collect(f,l));
     },
     concat:function(s)
     {
      return List.ofSeq(Seq.concat(s));
     },
     exists2:function(p,l1,l2)
     {
      return Arrays.exists2(p,Arrays.ofSeq(l1),Arrays.ofSeq(l2));
     },
     filter:function(p,l)
     {
      return List.ofSeq(Seq.filter(p,l));
     },
     fold2:function(f,s,l1,l2)
     {
      return Arrays.fold2(f,s,Arrays.ofSeq(l1),Arrays.ofSeq(l2));
     },
     foldBack:function(f,l,s)
     {
      var arr;
      arr=Arrays.ofSeq(l);
      return arr.reduceRight(function(s1,x)
      {
       return(f(x))(s1);
      },s);
     },
     foldBack2:function(f,l1,l2,s)
     {
      return Arrays.foldBack2(f,Arrays.ofSeq(l1),Arrays.ofSeq(l2),s);
     },
     forall2:function(p,l1,l2)
     {
      return Arrays.forall2(p,Arrays.ofSeq(l1),Arrays.ofSeq(l2));
     },
     init:function(s,f)
     {
      return List.ofArray(Arrays.init(s,f));
     },
     iter2:function(f,l1,l2)
     {
      return Arrays.iter2(f,Arrays.ofSeq(l1),Arrays.ofSeq(l2));
     },
     iteri2:function(f,l1,l2)
     {
      return Arrays.iteri2(f,Arrays.ofSeq(l1),Arrays.ofSeq(l2));
     },
     map:function(f,l)
     {
      return List.ofSeq(Seq.map(f,l));
     },
     map2:function(f,l1,l2)
     {
      return List.ofArray(Arrays.map2(f,Arrays.ofSeq(l1),Arrays.ofSeq(l2)));
     },
     map3:function(f,l1,l2,l3)
     {
      var array;
      array=Arrays.map2(function(func)
      {
       return func;
      },Arrays.map2(f,Arrays.ofSeq(l1),Arrays.ofSeq(l2)),Arrays.ofSeq(l3));
      return List.ofArray(array);
     },
     mapi:function(f,l)
     {
      return List.ofSeq(Seq.mapi(f,l));
     },
     mapi2:function(f,l1,l2)
     {
      return List.ofArray(Arrays.mapi2(f,Arrays.ofSeq(l1),Arrays.ofSeq(l2)));
     },
     max:function(l)
     {
      return Seq.reduce(function(e1)
      {
       return function(e2)
       {
        return Operators.Max(e1,e2);
       };
      },l);
     },
     maxBy:function(f,l)
     {
      return Seq.reduce(function(x)
      {
       return function(y)
       {
        if(Unchecked.Compare(f(x),f(y))===1)
         {
          return x;
         }
        else
         {
          return y;
         }
       };
      },l);
     },
     min:function(l)
     {
      return Seq.reduce(function(e1)
      {
       return function(e2)
       {
        return Operators.Min(e1,e2);
       };
      },l);
     },
     minBy:function(f,l)
     {
      return Seq.reduce(function(x)
      {
       return function(y)
       {
        if(Unchecked.Compare(f(x),f(y))===-1)
         {
          return x;
         }
        else
         {
          return y;
         }
       };
      },l);
     },
     ofArray:function(arr)
     {
      var r;
      r=Runtime.New(T1,{
       $:0
      });
      Runtime.For(0,arr.length-1,function(i)
      {
       r=Runtime.New(T1,{
        $:1,
        $0:arr[arr.length-i-1],
        $1:r
       });
      });
      return r;
     },
     ofSeq:function(s)
     {
      var r,e,x;
      r=[];
      e=Enumerator.Get(s);
      Runtime.While(function()
      {
       return e.MoveNext();
      },function()
      {
       r.unshift(e.get_Current());
      });
      x=r.slice(0);
      x.reverse();
      return List.ofArray(x);
     },
     partition:function(p,l)
     {
      var patternInput,b,a;
      patternInput=Arrays.partition(p,Arrays.ofSeq(l));
      b=patternInput[1];
      a=patternInput[0];
      return[List.ofArray(a),List.ofArray(b)];
     },
     permute:function(f,l)
     {
      return List.ofArray(Arrays.permute(f,Arrays.ofSeq(l)));
     },
     reduceBack:function(f,l)
     {
      var arr;
      arr=Arrays.ofSeq(l);
      return arr.reduceRight(function(s,x)
      {
       return(f(x))(s);
      });
     },
     replicate:function(size,value)
     {
      return List.ofArray(Arrays.create(size,value));
     },
     rev:function(l)
     {
      var a;
      a=Arrays.ofSeq(l);
      a.reverse();
      return List.ofArray(a);
     },
     scan:function(f,s,l)
     {
      return List.ofSeq(Seq.scan(f,s,l));
     },
     scanBack:function(f,l,s)
     {
      return List.ofArray(Arrays.scanBack(f,Arrays.ofSeq(l),s));
     },
     sort:function(l)
     {
      var a;
      a=Arrays.ofSeq(l);
      Arrays.sortInPlace(a);
      return List.ofArray(a);
     },
     sortBy:function(f,l)
     {
      return List.sortWith(function(x)
      {
       return function(y)
       {
        return Operators.Compare(f(x),f(y));
       };
      },l);
     },
     sortWith:function(f,l)
     {
      var a;
      a=Arrays.ofSeq(l);
      Arrays.sortInPlaceWith(f,a);
      return List.ofArray(a);
     },
     unzip:function(l)
     {
      var x,y,enumerator;
      x=[];
      y=[];
      enumerator=Enumerator.Get(l);
      Runtime.While(function()
      {
       return enumerator.MoveNext();
      },function()
      {
       var forLoopVar,b,a;
       forLoopVar=enumerator.get_Current(),(b=forLoopVar[1],(a=forLoopVar[0],(x.push(a),y.push(b))));
      });
      return[List.ofArray(x.slice(0)),List.ofArray(y.slice(0))];
     },
     unzip3:function(l)
     {
      var x,y,z,enumerator;
      x=[];
      y=[];
      z=[];
      enumerator=Enumerator.Get(l);
      Runtime.While(function()
      {
       return enumerator.MoveNext();
      },function()
      {
       var forLoopVar,c,b,a;
       forLoopVar=enumerator.get_Current(),(c=forLoopVar[2],(b=forLoopVar[1],(a=forLoopVar[0],(x.push(a),(y.push(b),z.push(c))))));
      });
      return[List.ofArray(x.slice(0)),List.ofArray(y.slice(0)),List.ofArray(z.slice(0))];
     },
     zip:function(l1,l2)
     {
      return List.ofArray(Arrays.zip(Arrays.ofSeq(l1),Arrays.ofSeq(l2)));
     },
     zip3:function(l1,l2,l3)
     {
      return List.ofArray(Arrays.zip3(Arrays.ofSeq(l1),Arrays.ofSeq(l2),Arrays.ofSeq(l3)));
     }
    },
    OperatorIntrinsics:{
     GetArraySlice:function(source,start,finish)
     {
      var matchValue,f,f1;
      matchValue=[start,finish];
      if(matchValue[0].$==0)
       {
        if(matchValue[1].$==1)
         {
          f=matchValue[1].$0;
          return source.slice(0,f+1);
         }
        else
         {
          return[];
         }
       }
      else
       {
        if(matchValue[1].$==0)
         {
          return source.slice(matchValue[0].$0);
         }
        else
         {
          f1=matchValue[1].$0;
          return source.slice(matchValue[0].$0,f1+1);
         }
       }
     },
     GetStringSlice:function(source,start,finish)
     {
      var matchValue,f,f1;
      matchValue=[start,finish];
      if(matchValue[0].$==0)
       {
        if(matchValue[1].$==1)
         {
          f=matchValue[1].$0;
          return source.slice(0,f+1);
         }
        else
         {
          return"";
         }
       }
      else
       {
        if(matchValue[1].$==0)
         {
          return source.slice(matchValue[0].$0);
         }
        else
         {
          f1=matchValue[1].$0;
          return source.slice(matchValue[0].$0,f1+1);
         }
       }
     }
    },
    Operators:{
     Compare:function(a,b)
     {
      return Unchecked.Compare(a,b);
     },
     Decrement:function(x)
     {
      x.contents=x.contents-1;
     },
     DefaultArg:function(x,d)
     {
      var x1;
      if(x.$==0)
       {
        return d;
       }
      else
       {
        x1=x.$0;
        return x1;
       }
     },
     FailWith:function(msg)
     {
      return Operators.Raise(new Error(msg));
     },
     Increment:function(x)
     {
      x.contents=x.contents+1;
     },
     KeyValue:function(kvp)
     {
      return[kvp.K,kvp.V];
     },
     Max:function(a,b)
     {
      if(Unchecked.Compare(a,b)===1)
       {
        return a;
       }
      else
       {
        return b;
       }
     },
     Min:function(a,b)
     {
      if(Unchecked.Compare(a,b)===-1)
       {
        return a;
       }
      else
       {
        return b;
       }
     },
     Pown:function(a,n)
     {
      var p;
      p=function(n1)
      {
       var b;
       if(n1===1)
        {
         return a;
        }
       else
        {
         if(n1%2===0)
          {
           b=p(n1/2>>0);
           return b*b;
          }
         else
          {
           return a*p(n1-1);
          }
        }
      };
      return p(n);
     },
     Raise:function($e)
     {
      var $0=this,$this=this;
      throw $e;
     },
     Sign:function(x)
     {
      if(x===0)
       {
        return 0;
       }
      else
       {
        if(x<0)
         {
          return-1;
         }
        else
         {
          return 1;
         }
       }
     },
     Truncate:function(x)
     {
      if(x<0)
       {
        return Math.ceil(x);
       }
      else
       {
        return Math.floor(x);
       }
     },
     Using:function(t,f)
     {
      try
      {
       return f(t);
      }
      finally
      {
       t.Dispose();
      }
     },
     range:function(min,max)
     {
      return Seq.init(1+max-min,function(x)
      {
       return x+min;
      });
     },
     step:function(min,step,max)
     {
      var s,x,x1,f;
      s=Operators.Sign(step);
      x=(x1=Seq.initInfinite(function(k)
      {
       return min+k*step;
      }),(f=function(source)
      {
       return Seq.takeWhile(function(k)
       {
        return s*(max-k)>=0;
       },source);
      },f(x1)));
      return x;
     }
    },
    Option:{
     bind:function(f,x)
     {
      if(x.$==0)
       {
        return{
         $:0
        };
       }
      else
       {
        return f(x.$0);
       }
     },
     exists:function(p,x)
     {
      if(x.$==0)
       {
        return false;
       }
      else
       {
        return p(x.$0);
       }
     },
     fold:function(f,s,x)
     {
      if(x.$==0)
       {
        return s;
       }
      else
       {
        return(f(s))(x.$0);
       }
     },
     foldBack:function(f,x,s)
     {
      var x1;
      if(x.$==0)
       {
        return s;
       }
      else
       {
        x1=x.$0;
        return(f(x1))(s);
       }
     },
     forall:function(p,x)
     {
      if(x.$==0)
       {
        return true;
       }
      else
       {
        return p(x.$0);
       }
     },
     iter:function(p,x)
     {
      if(x.$==0)
       {
        return null;
       }
      else
       {
        return p(x.$0);
       }
     },
     map:function(f,x)
     {
      var x1;
      if(x.$==0)
       {
        return{
         $:0
        };
       }
      else
       {
        x1=x.$0;
        return{
         $:1,
         $0:f(x1)
        };
       }
     },
     toArray:function(x)
     {
      var x1;
      if(x.$==0)
       {
        return[];
       }
      else
       {
        x1=x.$0;
        return[x1];
       }
     },
     toList:function(x)
     {
      var x1;
      if(x.$==0)
       {
        return Runtime.New(T1,{
         $:0
        });
       }
      else
       {
        x1=x.$0;
        return List.ofArray([x1]);
       }
     }
    },
    Pervasives:{
     NewFromList:function(fields)
     {
      var r,enumerator;
      r={};
      enumerator=Enumerator.Get(fields);
      Runtime.While(function()
      {
       return enumerator.MoveNext();
      },function()
      {
       var forLoopVar,v,k;
       forLoopVar=enumerator.get_Current(),(v=forLoopVar[1],(k=forLoopVar[0],r[k]=v));
      });
      return r;
     }
    },
    Queue:{
     Clear:function(a)
     {
      return a.splice(0,a.length);
     },
     Contains:function(a,el)
     {
      return Seq.exists(function(y)
      {
       return Unchecked.Equals(el,y);
      },a);
     },
     CopyTo:function(a,array,index)
     {
      return Arrays.blit(a,0,array,index,a.length);
     }
    },
    Remoting:{
     AjaxProvider:Runtime.Field(function()
     {
      return XhrProvider.New();
     }),
     Async:function(m,data)
     {
      var headers,payload,callback;
      headers=Remoting.makeHeaders(m);
      payload=Remoting.makePayload(data);
      callback=Runtime.Tupled(function(tupledArg)
      {
       var ok,err,arg00;
       ok=tupledArg[0];
       err=tupledArg[1];
       tupledArg[2];
       arg00=Remoting.EndPoint();
       return((function(arg20)
       {
        return function(arg30)
        {
         return function(arg40)
         {
          return Remoting.AjaxProvider().Async(arg00,headers,arg20,arg30,arg40);
         };
        };
       }(payload))(function(x)
       {
        return ok(Json.Activate(JSON.parse(x)));
       }))(err);
      });
      return Concurrency.FromContinuations(function(ok)
      {
       return function(no)
       {
        return callback([ok,no,function(value)
        {
         value;
        }]);
       };
      });
     },
     Call:function(m,data)
     {
      var data1,arg00,arg10;
      data1=(arg00=Remoting.EndPoint(),(arg10=Remoting.makeHeaders(m),function(arg20)
      {
       return Remoting.AjaxProvider().Sync(arg00,arg10,arg20);
      })(Remoting.makePayload(data)));
      return Json.Activate(JSON.parse(data1));
     },
     EndPoint:Runtime.Field(function()
     {
      return"?";
     }),
     Send:function(m,data)
     {
      var computation;
      computation=Concurrency.Bind(Remoting.Async(m,data),function()
      {
       return Concurrency.Return(null);
      });
      ({
       $:0
      });
      return Concurrency.Start(computation);
     },
     XhrProvider:Runtime.Class({
      Async:function(url,headers,data,ok,err)
      {
       return Remoting.ajax(true,url,headers,data,ok,err);
      },
      Sync:function(url,headers,data)
      {
       var res;
       res={
        contents:undefined
       };
       Remoting.ajax(false,url,headers,data,function(x)
       {
        res.contents=x;
       },function(e)
       {
        return Operators.Raise(e);
       });
       return res.contents;
      }
     },{
      New:function()
      {
       var r;
       r=Runtime.New(this,{});
       return r;
      }
     }),
     ajax:function($async,$url,$headers,$data,$ok,$err)
     {
      var $0=this,$this=this;
      var xhr=new Global.XMLHttpRequest();
      xhr.open("POST",$url,$async);
      for(var h in $headers){
       xhr.setRequestHeader(h,$headers[h]);
      }
      function k()
      {
       if(xhr.status==200)
        {
         $ok(xhr.responseText);
        }
       else
        {
         var msg="Response status is not 200: ";
         $err(new Global.Error(msg+xhr.status));
        }
      }
      if("onload"in xhr)
       {
        xhr.onload=xhr.onerror=xhr.onabort=k;
       }
      else
       {
        xhr.onreadystatechange=function()
        {
         if(xhr.readyState==4)
          {
           k();
          }
        };
       }
      xhr.send($data);
     },
     makeHeaders:function(m)
     {
      var headers;
      headers={};
      headers["content-type"]="application/json";
      headers["x-websharper-rpc"]=m;
      return headers;
     },
     makePayload:function(data)
     {
      return JSON.stringify(data);
     }
    },
    Seq:{
     append:function(s1,s2)
     {
      return Enumerable.Of(function()
      {
       var e1,next;
       e1=Enumerator.Get(s1);
       next=function(x)
       {
        var v,e2,v1;
        if(x.s.MoveNext())
         {
          v=x.s.get_Current();
          x.c=v;
          return true;
         }
        else
         {
          if(x.s===e1)
           {
            e2=Enumerator.Get(s2);
            x.s=e2;
            if(e2.MoveNext())
             {
              v1=e2.get_Current();
              x.c=v1;
              return true;
             }
            else
             {
              return false;
             }
           }
          else
           {
            return false;
           }
         }
       };
       return T.New(e1,null,next);
      });
     },
     average:function(s)
     {
      var patternInput,sum,count;
      patternInput=Seq.fold(Runtime.Tupled(function(tupledArg)
      {
       var n,s1;
       n=tupledArg[0];
       s1=tupledArg[1];
       return function(x)
       {
        return[n+1,s1+x];
       };
      }),[0,0],s);
      sum=patternInput[1];
      count=patternInput[0];
      return sum/count;
     },
     averageBy:function(f,s)
     {
      var patternInput,sum,count;
      patternInput=Seq.fold(Runtime.Tupled(function(tupledArg)
      {
       var n,s1;
       n=tupledArg[0];
       s1=tupledArg[1];
       return function(x)
       {
        return[n+1,s1+f(x)];
       };
      }),[0,0],s);
      sum=patternInput[1];
      count=patternInput[0];
      return sum/count;
     },
     cache:function(s)
     {
      var cache,_enum;
      cache=[];
      _enum=Enumerator.Get(s);
      return Enumerable.Of(function()
      {
       var next;
       next=function(e)
       {
        var v,v1,v2,v3;
        if(e.s+1<cache.length)
         {
          v=e.s+1;
          e.s=v;
          v1=cache[e.s];
          e.c=v1;
          return true;
         }
        else
         {
          if(_enum.MoveNext())
           {
            v2=e.s+1;
            e.s=v2;
            v3=_enum.get_Current();
            e.c=v3;
            cache.push(e.get_Current());
            return true;
           }
          else
           {
            return false;
           }
         }
       };
       return T.New(0,null,next);
      });
     },
     choose:function(f,s)
     {
      var f1,mapping;
      f1=(mapping=function(x)
      {
       var matchValue,v;
       matchValue=f(x);
       if(matchValue.$==0)
        {
         return Runtime.New(T1,{
          $:0
         });
        }
       else
        {
         v=matchValue.$0;
         return List.ofArray([v]);
        }
      },function(source)
      {
       return Seq.collect(mapping,source);
      });
      return f1(s);
     },
     collect:function(f,s)
     {
      return Seq.concat(Seq.map(f,s));
     },
     compareWith:function(f,s1,s2)
     {
      var e1,e2,r,loop;
      e1=Enumerator.Get(s1);
      e2=Enumerator.Get(s2);
      r=0;
      loop=true;
      Runtime.While(function()
      {
       if(loop)
        {
         return r===0;
        }
       else
        {
         return false;
        }
      },function()
      {
       var matchValue;
       matchValue=[e1.MoveNext(),e2.MoveNext()],matchValue[0]?matchValue[1]?r=(f(e1.get_Current()))(e2.get_Current()):r=1:matchValue[1]?r=-1:loop=false;
      });
      return r;
     },
     concat:function(ss)
     {
      return Seq.fold(function(source1)
      {
       return function(source2)
       {
        return Seq.append(source1,source2);
       };
      },Seq.empty(),ss);
     },
     countBy:function(f,s)
     {
      return Seq.delay(function()
      {
       var d,e,keys,x,x1,f1,mapping,f2;
       d={};
       e=Enumerator.Get(s);
       keys=[];
       Runtime.While(function()
       {
        return e.MoveNext();
       },function()
       {
        var k,h;
        k=f(e.get_Current()),(h=Unchecked.Hash(k),d.hasOwnProperty(h)?d[h]=d[h]+1:(keys.push(k),d[h]=1));
       });
       x=(x1=keys.slice(0),(f1=(mapping=function(k)
       {
        return[k,d[Unchecked.Hash(k)]];
       },function(array)
       {
        return array.map(function(x2)
        {
         return mapping(x2);
        });
       }),f1(x1)));
       f2=function(x2)
       {
        return x2;
       };
       return f2(x);
      });
     },
     delay:function(f)
     {
      return Enumerable.Of(function()
      {
       return Enumerator.Get(f(null));
      });
     },
     distinct:function(s)
     {
      return Seq.distinctBy(function(x)
      {
       return x;
      },s);
     },
     distinctBy:function(f,s)
     {
      return Enumerable.Of(function()
      {
       var _enum,seen,next;
       _enum=Enumerator.Get(s);
       seen={};
       next=function(e)
       {
        var cur,h,check,has;
        if(_enum.MoveNext())
         {
          cur=_enum.get_Current();
          h=function(c)
          {
           var x;
           x=f(c);
           return Unchecked.Hash(x);
          };
          check=function(c)
          {
           return seen.hasOwnProperty(h(c));
          };
          has=check(cur);
          Runtime.While(function()
          {
           if(has)
            {
             return _enum.MoveNext();
            }
           else
            {
             return false;
            }
          },function()
          {
           cur=_enum.get_Current(),has=check(cur);
          });
          if(has)
           {
            return false;
           }
          else
           {
            seen[h(cur)]=null;
            e.c=cur;
            return true;
           }
         }
        else
         {
          return false;
         }
       };
       return T.New(null,null,next);
      });
     },
     empty:function()
     {
      return[];
     },
     enumFinally:function(s,f)
     {
      return Enumerable.Of(function()
      {
       var e,next;
       e=Runtime.Try(function()
       {
        return Enumerator.Get(s);
       },function(e1)
       {
        f(null);
        return Operators.Raise(e1);
       });
       next=function(x)
       {
        var v,e1;
        try
        {
         if(e.MoveNext())
          {
           v=e.get_Current();
           x.c=v;
           return true;
          }
         else
          {
           f(null);
           return false;
          }
        }
        catch(e1)
        {
         f(null);
         return Operators.Raise(e1);
        }
       };
       return T.New(null,null,next);
      });
     },
     enumUsing:function(x,f)
     {
      return f(x);
     },
     enumWhile:function(f,s)
     {
      return Enumerable.Of(function()
      {
       var next;
       next=function(en)
       {
        var matchValue,e,v,v1,v2;
        matchValue=en.s;
        if(matchValue.$==1)
         {
          e=matchValue.$0;
          if(e.MoveNext())
           {
            v=e.get_Current();
            en.c=v;
            return true;
           }
          else
           {
            v1={
             $:0
            };
            en.s=v1;
            return next(en);
           }
         }
        else
         {
          if(f(null))
           {
            v2={
             $:1,
             $0:Enumerator.Get(s)
            };
            en.s=v2;
            return next(en);
           }
          else
           {
            return false;
           }
         }
       };
       return T.New({
        $:0
       },null,next);
      });
     },
     exists:function(p,s)
     {
      var e,r;
      e=Enumerator.Get(s);
      r=false;
      Runtime.While(function()
      {
       if(!r)
        {
         return e.MoveNext();
        }
       else
        {
         return false;
        }
      },function()
      {
       r=p(e.get_Current());
      });
      return r;
     },
     exists2:function(p,s1,s2)
     {
      var e1,e2,r;
      e1=Enumerator.Get(s1);
      e2=Enumerator.Get(s2);
      r=false;
      Runtime.While(function()
      {
       if(!r?e1.MoveNext():false)
        {
         return e2.MoveNext();
        }
       else
        {
         return false;
        }
      },function()
      {
       r=(p(e1.get_Current()))(e2.get_Current());
      });
      return r;
     },
     filter:function(f,s)
     {
      return Enumerable.Of(function()
      {
       var _enum,next;
       _enum=Enumerator.Get(s);
       next=function(e)
       {
        var loop,c,res;
        loop=_enum.MoveNext();
        c=_enum.get_Current();
        res=false;
        Runtime.While(function()
        {
         return loop;
        },function()
        {
         f(c)?(e.c=c,(res=true,loop=false)):_enum.MoveNext()?c=_enum.get_Current():loop=false;
        });
        return res;
       };
       return T.New(null,null,next);
      });
     },
     find:function(p,s)
     {
      var matchValue,x;
      matchValue=Seq.tryFind(p,s);
      if(matchValue.$==0)
       {
        return Operators.FailWith("KeyNotFoundException");
       }
      else
       {
        x=matchValue.$0;
        return x;
       }
     },
     findIndex:function(p,s)
     {
      var matchValue,x;
      matchValue=Seq.tryFindIndex(p,s);
      if(matchValue.$==0)
       {
        return Operators.FailWith("KeyNotFoundException");
       }
      else
       {
        x=matchValue.$0;
        return x;
       }
     },
     fold:function(f,x,s)
     {
      var r,e;
      r=x;
      e=Enumerator.Get(s);
      Runtime.While(function()
      {
       return e.MoveNext();
      },function()
      {
       r=(f(r))(e.get_Current());
      });
      return r;
     },
     forall:function(p,s)
     {
      return!Seq.exists(function(x)
      {
       return!p(x);
      },s);
     },
     forall2:function(p,s1,s2)
     {
      return!Seq.exists2(function(x)
      {
       return function(y)
       {
        return!(p(x))(y);
       };
      },s1,s2);
     },
     groupBy:function(f,s)
     {
      return Seq.delay(function()
      {
       var d,d1,keys,e;
       d={};
       d1={};
       keys=[];
       e=Enumerator.Get(s);
       Runtime.While(function()
       {
        return e.MoveNext();
       },function()
       {
        var c,k,h;
        c=e.get_Current(),(k=f(c),(h=Unchecked.Hash(k),(!d.hasOwnProperty(h)?keys.push(k):null,(d1[h]=k,d.hasOwnProperty(h)?d[h].push(c):d[h]=[c]))));
       });
       return keys.map(function(x)
       {
        return function(k)
        {
         return[k,d[Unchecked.Hash(k)]];
        }(x);
       });
      });
     },
     head:function(s)
     {
      var e;
      e=Enumerator.Get(s);
      if(e.MoveNext())
       {
        return e.get_Current();
       }
      else
       {
        return Seq.insufficient();
       }
     },
     init:function(n,f)
     {
      return Seq.take(n,Seq.initInfinite(f));
     },
     initInfinite:function(f)
     {
      return Enumerable.Of(function()
      {
       var next;
       next=function(e)
       {
        var v,v1;
        v=f(e.s);
        e.c=v;
        v1=e.s+1;
        e.s=v1;
        return true;
       };
       return T.New(0,null,next);
      });
     },
     insufficient:function()
     {
      return Operators.FailWith("The input sequence has an insufficient number of elements.");
     },
     isEmpty:function(s)
     {
      var e;
      e=Enumerator.Get(s);
      return!e.MoveNext();
     },
     iter:function(p,s)
     {
      return Seq.iteri(function()
      {
       return p;
      },s);
     },
     iter2:function(p,s1,s2)
     {
      var e1,e2;
      e1=Enumerator.Get(s1);
      e2=Enumerator.Get(s2);
      Runtime.While(function()
      {
       if(e1.MoveNext())
        {
         return e2.MoveNext();
        }
       else
        {
         return false;
        }
      },function()
      {
       (p(e1.get_Current()))(e2.get_Current());
      });
     },
     iteri:function(p,s)
     {
      var i,e;
      i=0;
      e=Enumerator.Get(s);
      Runtime.While(function()
      {
       return e.MoveNext();
      },function()
      {
       (p(i))(e.get_Current()),i=i+1;
      });
     },
     length:function(s)
     {
      var i,e;
      i=0;
      e=Enumerator.Get(s);
      Runtime.While(function()
      {
       return e.MoveNext();
      },function()
      {
       i=i+1;
      });
      return i;
     },
     map:function(f,s)
     {
      return Enumerable.Of(function()
      {
       var en,next;
       en=Enumerator.Get(s);
       next=function(e)
       {
        var v;
        if(en.MoveNext())
         {
          v=f(en.get_Current());
          e.c=v;
          return true;
         }
        else
         {
          return false;
         }
       };
       return T.New(null,null,next);
      });
     },
     mapi:function(f,s)
     {
      return Seq.mapi2(f,Seq.initInfinite(function(x)
      {
       return x;
      }),s);
     },
     mapi2:function(f,s1,s2)
     {
      return Enumerable.Of(function()
      {
       var e1,e2,next;
       e1=Enumerator.Get(s1);
       e2=Enumerator.Get(s2);
       next=function(e)
       {
        var v;
        if(e1.MoveNext()?e2.MoveNext():false)
         {
          v=(f(e1.get_Current()))(e2.get_Current());
          e.c=v;
          return true;
         }
        else
         {
          return false;
         }
       };
       return T.New(null,null,next);
      });
     },
     max:function(s)
     {
      return Seq.reduce(function(x)
      {
       return function(y)
       {
        if(Unchecked.Compare(x,y)>=0)
         {
          return x;
         }
        else
         {
          return y;
         }
       };
      },s);
     },
     maxBy:function(f,s)
     {
      return Seq.reduce(function(x)
      {
       return function(y)
       {
        if(Unchecked.Compare(f(x),f(y))>=0)
         {
          return x;
         }
        else
         {
          return y;
         }
       };
      },s);
     },
     min:function(s)
     {
      return Seq.reduce(function(x)
      {
       return function(y)
       {
        if(Unchecked.Compare(x,y)<=0)
         {
          return x;
         }
        else
         {
          return y;
         }
       };
      },s);
     },
     minBy:function(f,s)
     {
      return Seq.reduce(function(x)
      {
       return function(y)
       {
        if(Unchecked.Compare(f(x),f(y))<=0)
         {
          return x;
         }
        else
         {
          return y;
         }
       };
      },s);
     },
     nth:function(index,s)
     {
      var pos,e;
      if(index<0)
       {
        Operators.FailWith("negative index requested");
       }
      pos=-1;
      e=Enumerator.Get(s);
      Runtime.While(function()
      {
       return pos<index;
      },function()
      {
       !e.MoveNext()?Seq.insufficient():null,pos=pos+1;
      });
      return e.get_Current();
     },
     pairwise:function(s)
     {
      var x,f;
      x=Seq.windowed(2,s);
      f=function(source)
      {
       return Seq.map(function(x1)
       {
        return[x1[0],x1[1]];
       },source);
      };
      return f(x);
     },
     pick:function(p,s)
     {
      var matchValue,x;
      matchValue=Seq.tryPick(p,s);
      if(matchValue.$==0)
       {
        return Operators.FailWith("KeyNotFoundException");
       }
      else
       {
        x=matchValue.$0;
        return x;
       }
     },
     readOnly:function(s)
     {
      return Enumerable.Of(function()
      {
       return Enumerator.Get(s);
      });
     },
     reduce:function(f,source)
     {
      var e,r;
      e=Enumerator.Get(source);
      if(!e.MoveNext())
       {
        Operators.FailWith("The input sequence was empty");
       }
      r=e.get_Current();
      Runtime.While(function()
      {
       return e.MoveNext();
      },function()
      {
       r=(f(r))(e.get_Current());
      });
      return r;
     },
     scan:function(f,x,s)
     {
      return Enumerable.Of(function()
      {
       var en,next;
       en=Enumerator.Get(s);
       next=function(e)
       {
        var v;
        if(e.s)
         {
          if(en.MoveNext())
           {
            v=(f(e.get_Current()))(en.get_Current());
            e.c=v;
            return true;
           }
          else
           {
            return false;
           }
         }
        else
         {
          e.c=x;
          e.s=true;
          return true;
         }
       };
       return T.New(false,null,next);
      });
     },
     skip:function(n,s)
     {
      return Enumerable.Of(function()
      {
       var e;
       e=Enumerator.Get(s);
       Runtime.For(1,n,function()
       {
        !e.MoveNext()?Seq.insufficient():null;
       });
       return e;
      });
     },
     skipWhile:function(f,s)
     {
      return Enumerable.Of(function()
      {
       var e,empty,next;
       e=Enumerator.Get(s);
       empty=true;
       while(e.MoveNext()?f(e.get_Current()):false)
        {
         empty=false;
        }
       if(empty)
        {
         return Enumerator.Get(Seq.empty());
        }
       else
        {
         next=function(x)
         {
          var v,r,v1;
          if(x.s)
           {
            x.s=false;
            v=e.get_Current();
            x.c=v;
            return true;
           }
          else
           {
            r=e.MoveNext();
            v1=e.get_Current();
            x.c=v1;
            return r;
           }
         };
         return T.New(true,null,next);
        }
      });
     },
     sort:function(s)
     {
      return Seq.sortBy(function(x)
      {
       return x;
      },s);
     },
     sortBy:function(f,s)
     {
      return Seq.delay(function()
      {
       var array;
       array=Arrays.ofSeq(s);
       Arrays.sortInPlaceBy(f,array);
       return array;
      });
     },
     sum:function(s)
     {
      return Seq.fold(function(s1)
      {
       return function(x)
       {
        return s1+x;
       };
      },0,s);
     },
     sumBy:function(f,s)
     {
      return Seq.fold(function(s1)
      {
       return function(x)
       {
        return s1+f(x);
       };
      },0,s);
     },
     take:function(n,s)
     {
      return Enumerable.Of(function()
      {
       var e,next;
       e=Enumerator.Get(s);
       next=function(_enum)
       {
        var v,v1;
        if(_enum.s>=n)
         {
          return false;
         }
        else
         {
          if(e.MoveNext())
           {
            v=_enum.s+1;
            _enum.s=v;
            v1=e.get_Current();
            _enum.c=v1;
            return true;
           }
          else
           {
            e.Dispose();
            _enum.s=n;
            return false;
           }
         }
       };
       return T.New(0,null,next);
      });
     },
     takeWhile:function(f,s)
     {
      return Seq.delay(function()
      {
       return Seq.enumUsing(Enumerator.Get(s),function(e)
       {
        return Seq.enumWhile(function()
        {
         if(e.MoveNext())
          {
           return f(e.get_Current());
          }
         else
          {
           return false;
          }
        },Seq.delay(function()
        {
         return[e.get_Current()];
        }));
       });
      });
     },
     toArray:function(s)
     {
      var q,enumerator;
      q=[];
      enumerator=Enumerator.Get(s);
      Runtime.While(function()
      {
       return enumerator.MoveNext();
      },function()
      {
       q.push(enumerator.get_Current());
      });
      return q.slice(0);
     },
     toList:function(s)
     {
      return List.ofSeq(s);
     },
     truncate:function(n,s)
     {
      return Seq.delay(function()
      {
       return Seq.enumUsing(Enumerator.Get(s),function(e)
       {
        var i;
        i={
         contents:0
        };
        return Seq.enumWhile(function()
        {
         if(e.MoveNext())
          {
           return i.contents<n;
          }
         else
          {
           return false;
          }
        },Seq.delay(function()
        {
         Operators.Increment(i);
         return[e.get_Current()];
        }));
       });
      });
     },
     tryFind:function(ok,s)
     {
      var e,r;
      e=Enumerator.Get(s);
      r={
       $:0
      };
      Runtime.While(function()
      {
       if(r.$==0)
        {
         return e.MoveNext();
        }
       else
        {
         return false;
        }
      },function()
      {
       var x;
       x=e.get_Current(),ok(x)?r={
        $:1,
        $0:x
       }:null;
      });
      return r;
     },
     tryFindIndex:function(ok,s)
     {
      var e,loop,i;
      e=Enumerator.Get(s);
      loop=true;
      i=0;
      Runtime.While(function()
      {
       if(loop)
        {
         return e.MoveNext();
        }
       else
        {
         return false;
        }
      },function()
      {
       var x;
       x=e.get_Current(),ok(x)?loop=false:i=i+1;
      });
      if(loop)
       {
        return{
         $:0
        };
       }
      else
       {
        return{
         $:1,
         $0:i
        };
       }
     },
     tryPick:function(f,s)
     {
      var e,r;
      e=Enumerator.Get(s);
      r={
       $:0
      };
      Runtime.While(function()
      {
       if(Unchecked.Equals(r,{
        $:0
       }))
        {
         return e.MoveNext();
        }
       else
        {
         return false;
        }
      },function()
      {
       r=f(e.get_Current());
      });
      return r;
     },
     unfold:function(f,s)
     {
      return Enumerable.Of(function()
      {
       var next;
       next=function(e)
       {
        var matchValue,t,s1;
        matchValue=f(e.s);
        if(matchValue.$==0)
         {
          return false;
         }
        else
         {
          t=matchValue.$0[0];
          s1=matchValue.$0[1];
          e.c=t;
          e.s=s1;
          return true;
         }
       };
       return T.New(s,null,next);
      });
     },
     windowed:function(windowSize,s)
     {
      if(windowSize<=0)
       {
        Operators.FailWith("The input must be non-negative.");
       }
      return Seq.delay(function()
      {
       return Seq.enumUsing(Enumerator.Get(s),function(e)
       {
        var q;
        q=[];
        return Seq.append(Seq.enumWhile(function()
        {
         if(q.length<windowSize)
          {
           return e.MoveNext();
          }
         else
          {
           return false;
          }
        },Seq.delay(function()
        {
         q.push(e.get_Current());
         return Seq.empty();
        })),Seq.delay(function()
        {
         if(q.length===windowSize)
          {
           return Seq.append([q.slice(0)],Seq.delay(function()
           {
            return Seq.enumWhile(function()
            {
             return e.MoveNext();
            },Seq.delay(function()
            {
             q.shift();
             q.push(e.get_Current());
             return[q.slice(0)];
            }));
           }));
          }
         else
          {
           return Seq.empty();
          }
        }));
       });
      });
     },
     zip:function(s1,s2)
     {
      return Seq.mapi2(function(x)
      {
       return function(y)
       {
        return[x,y];
       };
      },s1,s2);
     },
     zip3:function(s1,s2,s3)
     {
      return Seq.mapi2(function(x)
      {
       return Runtime.Tupled(function(tupledArg)
       {
        var y,z;
        y=tupledArg[0];
        z=tupledArg[1];
        return[x,y,z];
       });
      },s1,Seq.zip(s2,s3));
     }
    },
    Stack:{
     Clear:function(stack)
     {
      return stack.splice(0,stack.length);
     },
     Contains:function(stack,el)
     {
      return Seq.exists(function(y)
      {
       return Unchecked.Equals(el,y);
      },stack);
     },
     CopyTo:function(stack,array,index)
     {
      return Arrays.blit(array,0,array,index,stack.length);
     }
    },
    Strings:{
     Compare:function(x,y)
     {
      return Operators.Compare(x,y);
     },
     CopyTo:function(s,o,d,off,ct)
     {
      return Arrays.blit(Strings.ToCharArray(s),o,d,off,ct);
     },
     EndsWith:function($x,$s)
     {
      var $0=this,$this=this;
      return $x.substring($x.length-$s.length)==$s;
     },
     IndexOf:function($s,$c,$i)
     {
      var $0=this,$this=this;
      return $s.indexOf(Global.String.fromCharCode($c),$i);
     },
     Insert:function($x,$index,$s)
     {
      var $0=this,$this=this;
      return $x.substring(0,$index-1)+$s+$x.substring($index);
     },
     IsNullOrEmpty:function($x)
     {
      var $0=this,$this=this;
      return $x==null||$x=="";
     },
     Join:function($sep,$values)
     {
      var $0=this,$this=this;
      return $values.join($sep);
     },
     LastIndexOf:function($s,$c,$i)
     {
      var $0=this,$this=this;
      return $s.lastIndexOf(Global.String.fromCharCode($c),$i);
     },
     PadLeft:function(s,n)
     {
      return Array(n-s.length+1).join(String.fromCharCode(32))+s;
     },
     PadRight:function(s,n)
     {
      return s+Array(n-s.length+1).join(String.fromCharCode(32));
     },
     RegexEscape:function($s)
     {
      var $0=this,$this=this;
      return $s.replace(/[-\/\\^$*+?.()|[\]{}]/g,"\\$&");
     },
     Remove:function($x,$ix,$ct)
     {
      var $0=this,$this=this;
      return $x.substring(0,$ix)+$x.substring($ix+$ct);
     },
     Replace:function(subject,search,replace)
     {
      var loop;
      loop=[];
      loop[1]=subject;
      loop[0]=1;
      Runtime.While(function()
      {
       return loop[0];
      },function()
      {
       var matchValue;
       matchValue=Strings.ReplaceOnce(loop[1],search,replace),matchValue===loop[1]?(loop[0]=0,loop[1]=matchValue):(loop[1]=matchValue,loop[0]=1);
      });
      return loop[1];
     },
     ReplaceChar:function(s,oldC,newC)
     {
      return Strings.Replace(s,String.fromCharCode(oldC),String.fromCharCode(newC));
     },
     ReplaceOnce:function($string,$search,$replace)
     {
      var $0=this,$this=this;
      return $string.replace($search,$replace);
     },
     Split:function(s,pat,opts)
     {
      var res;
      res=Strings.SplitWith(s,pat);
      if(opts===1)
       {
        return res.filter(function(x)
        {
         return function(x1)
         {
          return x1!=="";
         }(x);
        });
       }
      else
       {
        return res;
       }
     },
     SplitChars:function(s,sep,opts)
     {
      var re;
      re="["+Strings.RegexEscape(String.fromCharCode.apply(undefined,sep))+"]";
      return Strings.Split(s,new RegExp(re),opts);
     },
     SplitStrings:function(s,sep,opts)
     {
      var re;
      re=Strings.concat("|",sep.map(function(x)
      {
       return function(s1)
       {
        return Strings.RegexEscape(s1);
       }(x);
      }));
      return Strings.Split(s,new RegExp(re),opts);
     },
     SplitWith:function($str,$pat)
     {
      var $0=this,$this=this;
      return $str.split($pat);
     },
     StartsWith:function($t,$s)
     {
      var $0=this,$this=this;
      return $t.substring(0,$s.length)==$s;
     },
     Substring:function($s,$ix,$ct)
     {
      var $0=this,$this=this;
      return $s.substr($ix,$ct);
     },
     ToCharArray:function(s)
     {
      return Arrays.init(s.length,function(x)
      {
       return s.charCodeAt(x);
      });
     },
     ToCharArrayRange:function(s,startIndex,length)
     {
      return Arrays.init(length,function(i)
      {
       return s.charCodeAt(startIndex+i);
      });
     },
     Trim:function($s)
     {
      var $0=this,$this=this;
      return $s.replace(/^\s+/,"").replace(/\s+$/,"");
     },
     collect:function(f,s)
     {
      return Arrays.init(s.length,function(i)
      {
       return f(s.charCodeAt(i));
      }).join("");
     },
     concat:function(separator,strings)
     {
      return Seq.toArray(strings).join(separator);
     },
     exists:function(f,s)
     {
      return Seq.exists(f,Strings.protect(s));
     },
     forall:function(f,s)
     {
      return Seq.forall(f,Strings.protect(s));
     },
     init:function(count,f)
     {
      return Arrays.init(count,f).join("");
     },
     iter:function(f,s)
     {
      return Seq.iter(f,Strings.protect(s));
     },
     iteri:function(f,s)
     {
      return Seq.iteri(f,Strings.protect(s));
     },
     length:function(s)
     {
      return Strings.protect(s).length;
     },
     map:function(f,s)
     {
      return Strings.collect(function(x)
      {
       return String.fromCharCode(f(x));
      },Strings.protect(s));
     },
     mapi:function(f,s)
     {
      return Seq.toArray(Seq.mapi(function(i)
      {
       return function(x)
       {
        return String.fromCharCode((f(i))(x));
       };
      },s)).join("");
     },
     protect:function(s)
     {
      if(s===null)
       {
        return"";
       }
      else
       {
        return s;
       }
     },
     replicate:function(count,s)
     {
      return Strings.init(count,function()
      {
       return s;
      });
     }
    },
    Unchecked:{
     Compare:function(a,b)
     {
      var matchValue,matchValue1;
      if(a===b)
       {
        return 0;
       }
      else
       {
        matchValue=typeof a;
        if(matchValue==="undefined")
         {
          matchValue1=typeof b;
          if(matchValue1==="undefined")
           {
            return 0;
           }
          else
           {
            return-1;
           }
         }
        else
         {
          if(matchValue==="function")
           {
            return Operators.FailWith("Cannot compare function values.");
           }
          else
           {
            if(matchValue==="boolean")
             {
              if(a<b)
               {
                return-1;
               }
              else
               {
                return 1;
               }
             }
            else
             {
              if(matchValue==="number")
               {
                if(a<b)
                 {
                  return-1;
                 }
                else
                 {
                  return 1;
                 }
               }
              else
               {
                if(matchValue==="string")
                 {
                  if(a<b)
                   {
                    return-1;
                   }
                  else
                   {
                    return 1;
                   }
                 }
                else
                 {
                  if(a===null)
                   {
                    return-1;
                   }
                  else
                   {
                    if(b===null)
                     {
                      return 1;
                     }
                    else
                     {
                      if("CompareTo"in a)
                       {
                        return a.CompareTo(b);
                       }
                      else
                       {
                        if(a instanceof Array?b instanceof Array:false)
                         {
                          return Unchecked.compareArrays(a,b);
                         }
                        else
                         {
                          return Unchecked.compareArrays(JavaScript.GetFields(a),JavaScript.GetFields(b));
                         }
                       }
                     }
                   }
                 }
               }
             }
           }
         }
       }
     },
     Equals:function(a,b)
     {
      var matchValue;
      if(a===b)
       {
        return true;
       }
      else
       {
        matchValue=typeof a;
        if(matchValue==="object")
         {
          if(a===null)
           {
            return false;
           }
          else
           {
            if(b===null)
             {
              return false;
             }
            else
             {
              if("Equals"in a)
               {
                return a.Equals(b);
               }
              else
               {
                if(a instanceof Array?b instanceof Array:false)
                 {
                  return Unchecked.arrayEquals(a,b);
                 }
                else
                 {
                  return Unchecked.arrayEquals(JavaScript.GetFields(a),JavaScript.GetFields(b));
                 }
               }
             }
           }
         }
        else
         {
          return false;
         }
       }
     },
     Hash:function(o)
     {
      var matchValue;
      matchValue=typeof o;
      if(matchValue==="function")
       {
        return 0;
       }
      else
       {
        if(matchValue==="boolean")
         {
          if(o)
           {
            return 1;
           }
          else
           {
            return 0;
           }
         }
        else
         {
          if(matchValue==="number")
           {
            return o;
           }
          else
           {
            if(matchValue==="string")
             {
              return Unchecked.hashString(o);
             }
            else
             {
              if(matchValue==="object")
               {
                if(o==null)
                 {
                  return 0;
                 }
                else
                 {
                  if(o instanceof Array)
                   {
                    return Unchecked.hashArray(o);
                   }
                  else
                   {
                    return Unchecked.hashObject(o);
                   }
                 }
               }
              else
               {
                return 0;
               }
             }
           }
         }
       }
     },
     arrayEquals:function(a,b)
     {
      var eq,i;
      if(a.length===b.length)
       {
        eq=true;
        i=0;
        Runtime.While(function()
        {
         if(eq)
          {
           return i<a.length;
          }
         else
          {
           return false;
          }
        },function()
        {
         !Unchecked.Equals(a[i],b[i])?eq=false:null,i=i+1;
        });
        return eq;
       }
      else
       {
        return false;
       }
     },
     compareArrays:function(a,b)
     {
      var cmp,i;
      if(a.length<b.length)
       {
        return-1;
       }
      else
       {
        if(a.length>b.length)
         {
          return 1;
         }
        else
         {
          cmp=0;
          i=0;
          Runtime.While(function()
          {
           if(cmp===0)
            {
             return i<a.length;
            }
           else
            {
             return false;
            }
          },function()
          {
           cmp=Unchecked.Compare(a[i],b[i]),i=i+1;
          });
          return cmp;
         }
       }
     },
     hashArray:function(o)
     {
      var h;
      h=-34948909;
      Runtime.For(0,o.length-1,function(i)
      {
       h=Unchecked.hashMix(h,Unchecked.Hash(o[i]));
      });
      return h;
     },
     hashMix:function(x,y)
     {
      return(x<<5)+x+y;
     },
     hashObject:function(o)
     {
      var op_PlusPlus,h;
      if("GetHashCode"in o)
       {
        return o.GetHashCode();
       }
      else
       {
        op_PlusPlus=function(x,y)
        {
         return Unchecked.hashMix(x,y);
        };
        h={
         contents:0
        };
        JavaScript.ForEach(o,function(key)
        {
         h.contents=op_PlusPlus(op_PlusPlus(h.contents,Unchecked.hashString(key)),Unchecked.Hash(o[key]));
         return false;
        });
        return h.contents;
       }
     },
     hashString:function(s)
     {
      var hash;
      if(s===null)
       {
        return 0;
       }
      else
       {
        hash=5381;
        Runtime.For(0,s.length-1,function(i)
        {
         hash=Unchecked.hashMix(hash,s.charCodeAt(i)<<0);
        });
        return hash;
       }
     }
    },
    Util:{
     addListener:function(event,h)
     {
      event.Subscribe(Util.observer(h));
     },
     observer:function(h)
     {
      return{
       OnCompleted:function(value)
       {
        value;
       },
       OnError:function(value)
       {
        value;
       },
       OnNext:h
      };
     },
     subscribeTo:function(event,h)
     {
      return event.Subscribe(Util.observer(h));
     }
    }
   }
  }
 });
 Runtime.OnInit(function()
 {
  WebSharper=Runtime.Safe(Global.IntelliFactory.WebSharper);
  Arrays=Runtime.Safe(WebSharper.Arrays);
  Operators=Runtime.Safe(WebSharper.Operators);
  Number=Runtime.Safe(Global.Number);
  Array=Runtime.Safe(Global.Array);
  Seq=Runtime.Safe(WebSharper.Seq);
  Unchecked=Runtime.Safe(WebSharper.Unchecked);
  Enumerator=Runtime.Safe(WebSharper.Enumerator);
  Char=Runtime.Safe(WebSharper.Char);
  Util=Runtime.Safe(WebSharper.Util);
  Concurrency=Runtime.Safe(WebSharper.Concurrency);
  setTimeout=Runtime.Safe(Global.setTimeout);
  Date=Runtime.Safe(Global.Date);
  JavaScript=Runtime.Safe(WebSharper.JavaScript);
  Scheduler=Runtime.Safe(Concurrency.Scheduler);
  T=Runtime.Safe(Enumerator.T);
  Json=Runtime.Safe(WebSharper.Json);
  List=Runtime.Safe(WebSharper.List);
  T1=Runtime.Safe(List.T);
  Error=Runtime.Safe(Global.Error);
  Math=Runtime.Safe(Global.Math);
  Remoting=Runtime.Safe(WebSharper.Remoting);
  XhrProvider=Runtime.Safe(Remoting.XhrProvider);
  JSON=Runtime.Safe(Global.JSON);
  Enumerable=Runtime.Safe(WebSharper.Enumerable);
  Strings=Runtime.Safe(WebSharper.Strings);
  String=Runtime.Safe(Global.String);
  return RegExp=Runtime.Safe(Global.RegExp);
 });
 Runtime.OnLoad(function()
 {
  Remoting.EndPoint();
  Remoting.AjaxProvider();
  Concurrency.scheduler();
 });
}());
