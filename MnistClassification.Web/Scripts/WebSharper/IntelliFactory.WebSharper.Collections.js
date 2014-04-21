(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,WebSharper,Collections,BalancedTree,Operators,List,T,Seq,Arrays,Enumerator,JavaScript,DictionaryUtil,Dictionary,Unchecked,FSharpMap,Pair,Option,MapUtil,FSharpSet,SetModule,SetUtil,LinkedList,EnumeratorProxy,ListProxy,ResizeArray,ResizeArrayProxy;
 Runtime.Define(Global,{
  IntelliFactory:{
   WebSharper:{
    Collections:{
     BalancedTree:{
      Add:function(x,t)
      {
       return BalancedTree.Put(function()
       {
        return function(x1)
        {
         return x1;
        };
       },x,t);
      },
      Branch:function(node,left,right)
      {
       return{
        Node:node,
        Left:left,
        Right:right,
        Height:1+Operators.Max(left==null?0:left.Height,right==null?0:right.Height),
        Count:1+(left==null?0:left.Count)+(right==null?0:right.Count)
       };
      },
      Build:function(data,min,max)
      {
       var sz,center,left,right;
       sz=max-min+1;
       if(sz<=0)
        {
         return null;
        }
       else
        {
         center=(min+max)/2>>0;
         left=BalancedTree.Build(data,min,center-1);
         right=BalancedTree.Build(data,center+1,max);
         return BalancedTree.Branch(data[center],left,right);
        }
      },
      Contains:function(v,t)
      {
       return!((BalancedTree.Lookup(v,t))[0]==null);
      },
      Enumerate:function(flip,t)
      {
       var gen;
       gen=Runtime.Tupled(function(tupledArg)
       {
        var t1,spine,t2,spine1,other;
        t1=tupledArg[0];
        spine=tupledArg[1];
        if(t1==null)
         {
          if(spine.$==1)
           {
            t2=spine.$0[0];
            spine1=spine.$1;
            other=spine.$0[1];
            return{
             $:1,
             $0:[t2,[other,spine1]]
            };
           }
          else
           {
            return{
             $:0
            };
           }
         }
        else
         {
          if(flip)
           {
            return gen([t1.Right,Runtime.New(T,{
             $:1,
             $0:[t1.Node,t1.Left],
             $1:spine
            })]);
           }
          else
           {
            return gen([t1.Left,Runtime.New(T,{
             $:1,
             $0:[t1.Node,t1.Right],
             $1:spine
            })]);
           }
         }
       });
       return Seq.unfold(gen,[t,Runtime.New(T,{
        $:0
       })]);
      },
      Lookup:function(k,t)
      {
       var spine,t1,loop;
       spine=[];
       t1=t;
       loop=true;
       Runtime.While(function()
       {
        return loop;
       },function()
       {
        var matchValue;
        t1==null?loop=false:(matchValue=Operators.Compare(k,t1.Node),matchValue===0?loop=false:matchValue===1?(spine.unshift([true,t1.Node,t1.Left]),t1=t1.Right):(spine.unshift([false,t1.Node,t1.Right]),t1=t1.Left));
       });
       return[t1,spine];
      },
      OfSeq:function(data)
      {
       var data1;
       data1=Arrays.sort(Seq.toArray(Seq.distinct(data)));
       return BalancedTree.Build(data1,0,data1.length-1);
      },
      Put:function(combine,k,t)
      {
       var patternInput,t1,spine;
       patternInput=BalancedTree.Lookup(k,t);
       t1=patternInput[0];
       spine=patternInput[1];
       if(t1==null)
        {
         return BalancedTree.Rebuild(spine,BalancedTree.Branch(k,null,null));
        }
       else
        {
         return BalancedTree.Rebuild(spine,BalancedTree.Branch((combine(t1.Node))(k),t1.Left,t1.Right));
        }
      },
      Rebuild:function(spine,t)
      {
       var h,t1;
       h=function(x)
       {
        if(x==null)
         {
          return 0;
         }
        else
         {
          return x.Height;
         }
       };
       t1=t;
       Runtime.For(0,spine.length-1,function(i)
       {
        var matchValue,x,l,m,x1,r,m1;
        t1=(matchValue=spine[i],matchValue[0]?(x=matchValue[1],(l=matchValue[2],h(t1)>h(l)+1?h(t1.Left)===h(t1.Right)+1?(m=t1.Left,BalancedTree.Branch(m.Node,BalancedTree.Branch(x,l,m.Left),BalancedTree.Branch(t1.Node,m.Right,t1.Right))):BalancedTree.Branch(t1.Node,BalancedTree.Branch(x,l,t1.Left),t1.Right):BalancedTree.Branch(x,l,t1))):(x1=matchValue[1],(r=matchValue[2],h(t1)>h(r)+1?h(t1.Right)===h(t1.Left)+1?(m1=t1.Right,BalancedTree.Branch(m1.Node,BalancedTree.Branch(t1.Node,t1.Left,m1.Left),BalancedTree.Branch(x1,m1.Right,r))):BalancedTree.Branch(t1.Node,t1.Left,BalancedTree.Branch(x1,t1.Right,r)):BalancedTree.Branch(x1,t1,r))));
       });
       return t1;
      },
      Remove:function(k,src)
      {
       var patternInput,t,spine,t1,data,source,t2,t3;
       patternInput=BalancedTree.Lookup(k,src);
       t=patternInput[0];
       spine=patternInput[1];
       if(t==null)
        {
         return src;
        }
       else
        {
         if(t.Right==null)
          {
           return BalancedTree.Rebuild(spine,t.Left);
          }
         else
          {
           if(t.Left==null)
            {
             return BalancedTree.Rebuild(spine,t.Right);
            }
           else
            {
             t1=(data=(source=Seq.append((t2=t.Left,BalancedTree.Enumerate(false,t2)),(t3=t.Right,BalancedTree.Enumerate(false,t3))),Seq.toArray(source)),BalancedTree.Build(data,0,data.length-1));
             return BalancedTree.Rebuild(spine,t1);
            }
          }
        }
      },
      TryFind:function(v,t)
      {
       var x;
       x=(BalancedTree.Lookup(v,t))[0];
       if(x==null)
        {
         return{
          $:0
         };
        }
       else
        {
         return{
          $:1,
          $0:x.Node
         };
        }
      }
     },
     Dictionary:Runtime.Class({
      Add:function(k,v)
      {
       var h;
       h=this.hash.call(null,k);
       if(this.data.hasOwnProperty(h))
        {
         return Operators.FailWith("An item with the same key has already been added.");
        }
       else
        {
         this.data[h]={
          K:k,
          V:v
         };
         this.count=this.count+1;
        }
      },
      Clear:function()
      {
       this.data={};
       this.count=0;
      },
      ContainsKey:function(k)
      {
       return this.data.hasOwnProperty(this.hash.call(null,k));
      },
      GetEnumerator:function()
      {
       var f,arr;
       return Enumerator.Get((f=Runtime.Tupled(function(tuple)
       {
        return tuple[1];
       }),(arr=JavaScript.GetFields(this.data),arr.map(function(x)
       {
        return f(x);
       }))));
      },
      Remove:function(k)
      {
       var h;
       h=this.hash.call(null,k);
       if(this.data.hasOwnProperty(h))
        {
         JavaScript.Delete(this.data,h);
         this.count=this.count-1;
         return true;
        }
       else
        {
         return false;
        }
      },
      get_Item:function(k)
      {
       var k1,x;
       k1=this.hash.call(null,k);
       if(this.data.hasOwnProperty(k1))
        {
         x=this.data[k1];
         return x.V;
        }
       else
        {
         return DictionaryUtil.notPresent();
        }
      },
      set_Item:function(k,v)
      {
       var h;
       h=this.hash.call(null,k);
       if(!this.data.hasOwnProperty(h))
        {
         this.count=this.count+1;
        }
       this.data[h]={
        K:k,
        V:v
       };
      }
     },{
      New:function(dictionary,comparer)
      {
       var r;
       r=Runtime.New(this,Dictionary.New11(dictionary,function(x)
       {
        return function(y)
        {
         return comparer.Equals(x,y);
        };
       },function(x)
       {
        return comparer.GetHashCode(x);
       }));
       return r;
      },
      New1:function(capacity,comparer)
      {
       var r;
       r=Runtime.New(this,Dictionary.New3(comparer));
       return r;
      },
      New11:function(init,equals,hash)
      {
       var r,enumerator;
       r=Runtime.New(this,{});
       r.hash=hash;
       r.count=0;
       r.data={};
       enumerator=Enumerator.Get(init);
       Runtime.While(function()
       {
        return enumerator.MoveNext();
       },function()
       {
        var x,x1;
        x=enumerator.get_Current(),r.data[x1=x.K,r.hash.call(null,x1)]=x.V;
       });
       return r;
      },
      New12:function()
      {
       var r;
       r=Runtime.New(this,Dictionary.New21());
       return r;
      },
      New2:function(dictionary)
      {
       var r;
       r=Runtime.New(this,Dictionary.New11(dictionary,function(x)
       {
        return function(y)
        {
         return Unchecked.Equals(x,y);
        };
       },function(obj)
       {
        return Unchecked.Hash(obj);
       }));
       return r;
      },
      New21:function()
      {
       var r;
       r=Runtime.New(this,Dictionary.New11([],function(x)
       {
        return function(y)
        {
         return Unchecked.Equals(x,y);
        };
       },function(obj)
       {
        return Unchecked.Hash(obj);
       }));
       return r;
      },
      New3:function(comparer)
      {
       var r;
       r=Runtime.New(this,Dictionary.New11([],function(x)
       {
        return function(y)
        {
         return comparer.Equals(x,y);
        };
       },function(x)
       {
        return comparer.GetHashCode(x);
       }));
       return r;
      }
     }),
     DictionaryUtil:{
      notPresent:function()
      {
       return Operators.FailWith("The given key was not present in the dictionary.");
      }
     },
     FSharpMap:Runtime.Class({
      Add:function(k,v)
      {
       var x,f,x1;
       return FSharpMap.New1((x=this.tree,(f=(x1=Runtime.New(Pair,{
        Key:k,
        Value:v
       }),function(t)
       {
        return BalancedTree.Add(x1,t);
       }),f(x))));
      },
      CompareTo:function(other)
      {
       return Seq.compareWith(function(x)
       {
        return function(y)
        {
         return Operators.Compare(x,y);
        };
       },this,other);
      },
      ContainsKey:function(k)
      {
       var x,f,v;
       x=this.tree;
       f=(v=Runtime.New(Pair,{
        Key:k,
        Value:undefined
       }),function(t)
       {
        return BalancedTree.Contains(v,t);
       });
       return f(x);
      },
      Equals:function(other)
      {
       if(this.get_Count()===other.get_Count())
        {
         return Seq.forall2(function(x)
         {
          return function(y)
          {
           return Unchecked.Equals(x,y);
          };
         },this,other);
        }
       else
        {
         return false;
        }
      },
      GetEnumerator:function()
      {
       var x,t,f;
       return Enumerator.Get((x=(t=this.tree,BalancedTree.Enumerate(false,t)),(f=function(source)
       {
        return Seq.map(function(kv)
        {
         return{
          K:kv.Key,
          V:kv.Value
         };
        },source);
       },f(x))));
      },
      GetHashCode:function()
      {
       return Unchecked.Hash(Seq.toArray(this));
      },
      Remove:function(k)
      {
       var x,f,k1;
       return FSharpMap.New1((x=this.tree,(f=(k1=Runtime.New(Pair,{
        Key:k,
        Value:undefined
       }),function(src)
       {
        return BalancedTree.Remove(k1,src);
       }),f(x))));
      },
      TryFind:function(k)
      {
       var x,x1,f,v,f1;
       x=(x1=this.tree,(f=(v=Runtime.New(Pair,{
        Key:k,
        Value:undefined
       }),function(t)
       {
        return BalancedTree.TryFind(v,t);
       }),f(x1)));
       f1=function(option)
       {
        return Option.map(function(kv)
        {
         return kv.Value;
        },option);
       };
       return f1(x);
      },
      get_Count:function()
      {
       var tree;
       tree=this.tree;
       if(tree==null)
        {
         return 0;
        }
       else
        {
         return tree.Count;
        }
      },
      get_IsEmpty:function()
      {
       return this.tree==null;
      },
      get_Item:function(k)
      {
       var matchValue,v;
       matchValue=this.TryFind(k);
       if(matchValue.$==0)
        {
         return Operators.FailWith("The given key was not present in the dictionary.");
        }
       else
        {
         v=matchValue.$0;
         return v;
        }
      },
      get_Tree:function()
      {
       return this.tree;
      }
     },{
      New:function(s)
      {
       var r;
       r=Runtime.New(this,FSharpMap.New1(MapUtil.fromSeq(s)));
       return r;
      },
      New1:function(tree)
      {
       var r;
       r=Runtime.New(this,{});
       r.tree=tree;
       return r;
      }
     }),
     FSharpSet:Runtime.Class({
      Add:function(x)
      {
       return FSharpSet.New1(BalancedTree.Add(x,this.tree));
      },
      CompareTo:function(other)
      {
       return Seq.compareWith(function(e1)
       {
        return function(e2)
        {
         return Operators.Compare(e1,e2);
        };
       },this,other);
      },
      Contains:function(v)
      {
       return BalancedTree.Contains(v,this.tree);
      },
      Equals:function(other)
      {
       if(this.get_Count()===other.get_Count())
        {
         return Seq.forall2(function(x)
         {
          return function(y)
          {
           return Unchecked.Equals(x,y);
          };
         },this,other);
        }
       else
        {
         return false;
        }
      },
      GetEnumerator:function()
      {
       var t;
       return Enumerator.Get((t=this.tree,BalancedTree.Enumerate(false,t)));
      },
      GetHashCode:function()
      {
       return-1741749453+Unchecked.Hash(Seq.toArray(this));
      },
      IsProperSubsetOf:function(s)
      {
       if(this.IsSubsetOf(s))
        {
         return this.get_Count()<s.get_Count();
        }
       else
        {
         return false;
        }
      },
      IsProperSupersetOf:function(s)
      {
       if(this.IsSupersetOf(s))
        {
         return this.get_Count()>s.get_Count();
        }
       else
        {
         return false;
        }
      },
      IsSubsetOf:function(s)
      {
       return Seq.forall(function(arg00)
       {
        return s.Contains(arg00);
       },this);
      },
      IsSupersetOf:function(s)
      {
       var _this=this;
       return Seq.forall(function(arg00)
       {
        return _this.Contains(arg00);
       },s);
      },
      Remove:function(v)
      {
       return FSharpSet.New1(BalancedTree.Remove(v,this.tree));
      },
      add:function(x)
      {
       var a,t;
       a=Seq.append(this,x);
       t=BalancedTree.OfSeq(a);
       return FSharpSet.New1(t);
      },
      get_Count:function()
      {
       var tree;
       tree=this.tree;
       if(tree==null)
        {
         return 0;
        }
       else
        {
         return tree.Count;
        }
      },
      get_IsEmpty:function()
      {
       return this.tree==null;
      },
      get_MaximumElement:function()
      {
       var t;
       return Seq.head((t=this.tree,BalancedTree.Enumerate(true,t)));
      },
      get_MinimumElement:function()
      {
       var t;
       return Seq.head((t=this.tree,BalancedTree.Enumerate(false,t)));
      },
      get_Tree:function()
      {
       return this.tree;
      },
      sub:function(x)
      {
       return SetModule.Filter(function(x1)
       {
        return!x.Contains(x1);
       },this);
      }
     },{
      New:function(s)
      {
       var r;
       r=Runtime.New(this,FSharpSet.New1(SetUtil.ofSeq(s)));
       return r;
      },
      New1:function(tree)
      {
       var r;
       r=Runtime.New(this,{});
       r.tree=tree;
       return r;
      }
     }),
     LinkedList:{
      EnumeratorProxy:Runtime.Class({
       Dispose:function()
       {
        return null;
       },
       MoveNext:function()
       {
        this.c=this.c.n;
        return!Unchecked.Equals(this.c,null);
       },
       get_Current:function()
       {
        return this.c.v;
       }
      },{
       New:function(l)
       {
        var r;
        r=Runtime.New(this,{});
        r.c=l;
        return r;
       }
      }),
      ListProxy:Runtime.Class({
       AddAfter:function(after,value)
       {
        var before,node;
        before=after.n;
        node={
         p:after,
         n:before,
         v:value
        };
        if(Unchecked.Equals(after.n,null))
         {
          this.p=node;
         }
        after.n=node;
        node;
        if(!Unchecked.Equals(before,null))
         {
          before.p=node;
          node;
         }
        this.c=this.c+1;
        return node;
       },
       AddBefore:function(before,value)
       {
        var after,node;
        after=before.p;
        node={
         p:after,
         n:before,
         v:value
        };
        if(Unchecked.Equals(before.p,null))
         {
          this.n=node;
         }
        before.p=node;
        node;
        if(!Unchecked.Equals(after,null))
         {
          after.n=node;
          node;
         }
        this.c=this.c+1;
        return node;
       },
       AddFirst:function(value)
       {
        var node;
        if(this.c===0)
         {
          node={
           p:null,
           n:null,
           v:value
          };
          this.n=node;
          this.p=this.n;
          this.c=1;
          return node;
         }
        else
         {
          return this.AddBefore(this.n,value);
         }
       },
       AddLast:function(value)
       {
        var node;
        if(this.c===0)
         {
          node={
           p:null,
           n:null,
           v:value
          };
          this.n=node;
          this.p=this.n;
          this.c=1;
          return node;
         }
        else
         {
          return this.AddAfter(this.p,value);
         }
       },
       Clear:function()
       {
        this.c=0;
        this.n=null;
        this.p=null;
       },
       Contains:function(value)
       {
        var found,node;
        found=false;
        node=this.n;
        Runtime.While(function()
        {
         if(!Unchecked.Equals(node,null))
          {
           return!found;
          }
         else
          {
           return false;
          }
        },function()
        {
         node.v==value?found=true:node=node.n;
        });
        return found;
       },
       Find:function(value)
       {
        var node;
        node=this.n;
        Runtime.While(function()
        {
         if(!Unchecked.Equals(node,null))
          {
           return node.v!=value;
          }
         else
          {
           return false;
          }
        },function()
        {
         node=node.n;
        });
        if(node==value)
         {
          return node;
         }
        else
         {
          return null;
         }
       },
       FindLast:function(value)
       {
        var node;
        node=this.p;
        Runtime.While(function()
        {
         if(!Unchecked.Equals(node,null))
          {
           return node.v!=value;
          }
         else
          {
           return false;
          }
        },function()
        {
         node=node.p;
        });
        if(node==value)
         {
          return node;
         }
        else
         {
          return null;
         }
       },
       GetEnumerator:function()
       {
        return EnumeratorProxy.New(this);
       },
       Remove:function(node)
       {
        var before,after;
        before=node.p;
        after=node.n;
        if(Unchecked.Equals(before,null))
         {
          this.n=after;
         }
        else
         {
          before.n=after;
          after;
         }
        if(Unchecked.Equals(after,null))
         {
          this.p=before;
         }
        else
         {
          after.p=before;
          before;
         }
        this.c=this.c-1;
       },
       Remove1:function(value)
       {
        var node;
        node=this.Find(value);
        if(Unchecked.Equals(node,null))
         {
          return false;
         }
        else
         {
          this.Remove(node);
          return true;
         }
       },
       RemoveFirst:function()
       {
        return this.Remove(this.n);
       },
       RemoveLast:function()
       {
        return this.Remove(this.p);
       },
       get_Count:function()
       {
        return this.c;
       },
       get_First:function()
       {
        return this.n;
       },
       get_Last:function()
       {
        return this.p;
       }
      },{
       New:function()
       {
        var r;
        r=Runtime.New(this,ListProxy.New1(Seq.empty()));
        return r;
       },
       New1:function(coll)
       {
        var r,ie;
        r=Runtime.New(this,{});
        r.c=0;
        r.n=null;
        r.p=null;
        ie=Enumerator.Get(coll);
        if(ie.MoveNext())
         {
          r.n={
           p:null,
           n:null,
           v:ie.get_Current()
          };
          r.p=r.n;
          r.c=1;
         }
        Runtime.While(function()
        {
         return ie.MoveNext();
        },function()
        {
         var node,_;
         node={
          p:r.p,
          n:null,
          v:ie.get_Current()
         },(_=r.p,(_.n=node,node),(r.p=node,r.c=r.c+1));
        });
        return r;
       }
      })
     },
     MapModule:{
      Exists:function(f,m)
      {
       var f1,predicate;
       f1=(predicate=function(kv)
       {
        return(f(kv.K))(kv.V);
       },function(source)
       {
        return Seq.exists(predicate,source);
       });
       return f1(m);
      },
      Filter:function(f,m)
      {
       var t,data,source,x,t1,f1;
       t=(data=(source=(x=(t1=m.get_Tree(),BalancedTree.Enumerate(false,t1)),(f1=function(source1)
       {
        return Seq.filter(function(kv)
        {
         return(f(kv.Key))(kv.Value);
        },source1);
       },f1(x))),Seq.toArray(source)),BalancedTree.Build(data,0,data.length-1));
       return FSharpMap.New1(t);
      },
      FindKey:function(f,m)
      {
       var f1,chooser;
       f1=(chooser=function(kv)
       {
        if((f(kv.K))(kv.V))
         {
          return{
           $:1,
           $0:kv.K
          };
         }
        else
         {
          return{
           $:0
          };
         }
       },function(source)
       {
        return Seq.pick(chooser,source);
       });
       return f1(m);
      },
      Fold:function(f,s,m)
      {
       var x,t,f1;
       x=(t=m.get_Tree(),BalancedTree.Enumerate(false,t));
       f1=function(source)
       {
        return Seq.fold(function(s1)
        {
         return function(kv)
         {
          return((f(s1))(kv.Key))(kv.Value);
         };
        },s,source);
       };
       return f1(x);
      },
      FoldBack:function(f,m,s)
      {
       var x,t,f1;
       x=(t=m.get_Tree(),BalancedTree.Enumerate(true,t));
       f1=function(source)
       {
        return Seq.fold(function(s1)
        {
         return function(kv)
         {
          return((f(kv.Key))(kv.Value))(s1);
         };
        },s,source);
       };
       return f1(x);
      },
      ForAll:function(f,m)
      {
       var f1,predicate;
       f1=(predicate=function(kv)
       {
        return(f(kv.K))(kv.V);
       },function(source)
       {
        return Seq.forall(predicate,source);
       });
       return f1(m);
      },
      Iterate:function(f,m)
      {
       var f1,action;
       f1=(action=function(kv)
       {
        return(f(kv.K))(kv.V);
       },function(source)
       {
        return Seq.iter(action,source);
       });
       return f1(m);
      },
      Map:function(f,m)
      {
       var t,data,x,t1,f1;
       t=(data=(x=(t1=m.get_Tree(),BalancedTree.Enumerate(false,t1)),(f1=function(source)
       {
        return Seq.map(function(kv)
        {
         return Runtime.New(Pair,{
          Key:kv.Key,
          Value:(f(kv.Key))(kv.Value)
         });
        },source);
       },f1(x))),BalancedTree.OfSeq(data));
       return FSharpMap.New1(t);
      },
      OfArray:function(a)
      {
       var t,data,f,mapping;
       t=(data=(f=(mapping=Runtime.Tupled(function(tupledArg)
       {
        var k,v;
        k=tupledArg[0];
        v=tupledArg[1];
        return Runtime.New(Pair,{
         Key:k,
         Value:v
        });
       }),function(source)
       {
        return Seq.map(mapping,source);
       }),f(a)),BalancedTree.OfSeq(data));
       return FSharpMap.New1(t);
      },
      Partition:function(f,m)
      {
       var patternInput,x,t,f1,y,x1;
       patternInput=(x=Seq.toArray((t=m.get_Tree(),BalancedTree.Enumerate(false,t))),(f1=function(array)
       {
        return Arrays.partition(function(kv)
        {
         return(f(kv.Key))(kv.Value);
        },array);
       },f1(x)));
       y=patternInput[1];
       x1=patternInput[0];
       return[FSharpMap.New1(BalancedTree.Build(x1,0,x1.length-1)),FSharpMap.New1(BalancedTree.Build(y,0,y.length-1))];
      },
      Pick:function(f,m)
      {
       var f1,chooser;
       f1=(chooser=function(kv)
       {
        return(f(kv.K))(kv.V);
       },function(source)
       {
        return Seq.pick(chooser,source);
       });
       return f1(m);
      },
      ToSeq:function(m)
      {
       var x,t,f;
       x=(t=m.get_Tree(),BalancedTree.Enumerate(false,t));
       f=function(source)
       {
        return Seq.map(function(kv)
        {
         return[kv.Key,kv.Value];
        },source);
       };
       return f(x);
      },
      TryFind:function(k,m)
      {
       return m.TryFind(k);
      },
      TryFindKey:function(f,m)
      {
       var f1,chooser;
       f1=(chooser=function(kv)
       {
        if((f(kv.K))(kv.V))
         {
          return{
           $:1,
           $0:kv.K
          };
         }
        else
         {
          return{
           $:0
          };
         }
       },function(source)
       {
        return Seq.tryPick(chooser,source);
       });
       return f1(m);
      },
      TryPick:function(f,m)
      {
       var f1,chooser;
       f1=(chooser=function(kv)
       {
        return(f(kv.K))(kv.V);
       },function(source)
       {
        return Seq.tryPick(chooser,source);
       });
       return f1(m);
      }
     },
     MapUtil:{
      fromSeq:function(s)
      {
       var a;
       a=Seq.toArray(Seq.delay(function()
       {
        return Seq.collect(Runtime.Tupled(function(matchValue)
        {
         var v,k;
         v=matchValue[1];
         k=matchValue[0];
         return[Runtime.New(Pair,{
          Key:k,
          Value:v
         })];
        }),Seq.distinctBy(Runtime.Tupled(function(tuple)
        {
         return tuple[0];
        }),s));
       }));
       Arrays.sortInPlace(a);
       return BalancedTree.Build(a,0,a.length-1);
      }
     },
     Pair:Runtime.Class({
      CompareTo:function(other)
      {
       return Operators.Compare(this.Key,other.Key);
      },
      Equals:function(other)
      {
       return Unchecked.Equals(this.Key,other.Key);
      },
      GetHashCode:function()
      {
       return Unchecked.Hash(this.Key);
      }
     }),
     ResizeArray:{
      ResizeArrayProxy:Runtime.Class({
       Add:function(x)
       {
        return this.arr.push(x);
       },
       AddRange:function(x)
       {
        var _this=this;
        return Seq.iter(function(arg00)
        {
         return _this.Add(arg00);
        },x);
       },
       Clear:function()
       {
        var value;
        value=ResizeArray.splice(this.arr,0,this.arr.length,[]);
        value;
       },
       CopyTo:function(arr)
       {
        return this.CopyTo1(arr,0);
       },
       CopyTo1:function(arr,offset)
       {
        return this.CopyTo2(0,arr,offset,this.get_Count());
       },
       CopyTo2:function(index,target,offset,count)
       {
        return Arrays.blit(this.arr,index,target,offset,count);
       },
       GetEnumerator:function()
       {
        return Enumerator.Get(this.arr);
       },
       GetRange:function(index,count)
       {
        return ResizeArrayProxy.New3(Arrays.sub(this.arr,index,count));
       },
       Insert:function(index,items)
       {
        var value;
        value=ResizeArray.splice(this.arr,index,0,[items]);
        value;
       },
       InsertRange:function(index,items)
       {
        var value;
        value=ResizeArray.splice(this.arr,index,0,Seq.toArray(items));
        value;
       },
       RemoveAt:function(x)
       {
        var value;
        value=ResizeArray.splice(this.arr,x,1,[]);
        value;
       },
       RemoveRange:function(index,count)
       {
        var value;
        value=ResizeArray.splice(this.arr,index,count,[]);
        value;
       },
       Reverse:function()
       {
        return this.arr.reverse();
       },
       Reverse1:function(index,count)
       {
        return Arrays.reverse(this.arr,index,count);
       },
       ToArray:function()
       {
        return this.arr.slice();
       },
       get_Count:function()
       {
        return this.arr.length;
       },
       get_Item:function(x)
       {
        return this.arr[x];
       },
       set_Item:function(x,v)
       {
        this.arr[x]=v;
       }
      },{
       New:function(el)
       {
        var r;
        r=Runtime.New(this,ResizeArrayProxy.New3(Seq.toArray(el)));
        return r;
       },
       New1:function()
       {
        var r;
        r=Runtime.New(this,ResizeArrayProxy.New3([]));
        return r;
       },
       New2:function()
       {
        var r;
        r=Runtime.New(this,ResizeArrayProxy.New3([]));
        return r;
       },
       New3:function(arr)
       {
        var r;
        r=Runtime.New(this,{});
        r.arr=arr;
        return r;
       }
      }),
      splice:function($arr,$index,$howMany,$items)
      {
       var $0=this,$this=this;
       return Global.Array.prototype.splice.apply($arr,[$index,$howMany].concat($items));
      }
     },
     SetModule:{
      Filter:function(f,s)
      {
       var data;
       return FSharpSet.New1((data=Seq.toArray(Seq.filter(f,s)),BalancedTree.Build(data,0,data.length-1)));
      },
      FoldBack:function(f,a,s)
      {
       var t;
       return Seq.fold(function(s1)
       {
        return function(x)
        {
         return(f(x))(s1);
        };
       },s,(t=a.get_Tree(),BalancedTree.Enumerate(true,t)));
      },
      Partition:function(f,a)
      {
       var patternInput,y,x,t,t1;
       patternInput=Arrays.partition(f,Seq.toArray(a));
       y=patternInput[1];
       x=patternInput[0];
       return[(t=BalancedTree.OfSeq(x),FSharpSet.New1(t)),(t1=BalancedTree.OfSeq(y),FSharpSet.New1(t1))];
      }
     },
     SetUtil:{
      ofSeq:function(s)
      {
       var a;
       a=Seq.toArray(s);
       Arrays.sortInPlace(a);
       return BalancedTree.Build(a,0,a.length-1);
      }
     }
    }
   }
  }
 });
 Runtime.OnInit(function()
 {
  WebSharper=Runtime.Safe(Global.IntelliFactory.WebSharper);
  Collections=Runtime.Safe(WebSharper.Collections);
  BalancedTree=Runtime.Safe(Collections.BalancedTree);
  Operators=Runtime.Safe(WebSharper.Operators);
  List=Runtime.Safe(WebSharper.List);
  T=Runtime.Safe(List.T);
  Seq=Runtime.Safe(WebSharper.Seq);
  Arrays=Runtime.Safe(WebSharper.Arrays);
  Enumerator=Runtime.Safe(WebSharper.Enumerator);
  JavaScript=Runtime.Safe(WebSharper.JavaScript);
  DictionaryUtil=Runtime.Safe(Collections.DictionaryUtil);
  Dictionary=Runtime.Safe(Collections.Dictionary);
  Unchecked=Runtime.Safe(WebSharper.Unchecked);
  FSharpMap=Runtime.Safe(Collections.FSharpMap);
  Pair=Runtime.Safe(Collections.Pair);
  Option=Runtime.Safe(WebSharper.Option);
  MapUtil=Runtime.Safe(Collections.MapUtil);
  FSharpSet=Runtime.Safe(Collections.FSharpSet);
  SetModule=Runtime.Safe(Collections.SetModule);
  SetUtil=Runtime.Safe(Collections.SetUtil);
  LinkedList=Runtime.Safe(Collections.LinkedList);
  EnumeratorProxy=Runtime.Safe(LinkedList.EnumeratorProxy);
  ListProxy=Runtime.Safe(LinkedList.ListProxy);
  ResizeArray=Runtime.Safe(Collections.ResizeArray);
  return ResizeArrayProxy=Runtime.Safe(ResizeArray.ResizeArrayProxy);
 });
 Runtime.OnLoad(function()
 {
 });
}());
