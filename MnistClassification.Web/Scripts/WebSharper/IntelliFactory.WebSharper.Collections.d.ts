declare module IntelliFactory {
    module WebSharper {
        module Collections {
            module LinkedList {
                interface NodeProxy<_T1> {
                }
                interface EnumeratorProxy<_T1> {
                    MoveNext(): boolean;
                    Dispose(): void;
                    get_Current(): _T1;
                }
                interface ListProxy<_T1> {
                    AddAfter(after: _LinkedList.NodeProxy<_T1>, value: _T1): _LinkedList.NodeProxy<_T1>;
                    AddBefore(before: _LinkedList.NodeProxy<_T1>, value: _T1): _LinkedList.NodeProxy<_T1>;
                    AddFirst(value: _T1): _LinkedList.NodeProxy<_T1>;
                    AddLast(value: _T1): _LinkedList.NodeProxy<_T1>;
                    Clear(): void;
                    Contains<_M1>(value: _M1): boolean;
                    Find(value: _T1): _LinkedList.NodeProxy<_T1>;
                    FindLast(value: _T1): _LinkedList.NodeProxy<_T1>;
                    GetEnumerator(): _LinkedList.EnumeratorProxy<_T1>;
                    Remove(node: _LinkedList.NodeProxy<_T1>): void;
                    Remove1(value: _T1): boolean;
                    RemoveFirst(): void;
                    RemoveLast(): void;
                    get_Count(): number;
                    get_First(): _LinkedList.NodeProxy<_T1>;
                    get_Last(): _LinkedList.NodeProxy<_T1>;
                }
            }
            module ResizeArray {
                interface ResizeArrayProxy<_T1> {
                    GetEnumerator(): _WebSharper.IEnumeratorProxy<_WebSharper.ObjectProxy>;
                    Add(x: _T1): void;
                    AddRange(x: _WebSharper.seq<_T1>): void;
                    Clear(): void;
                    CopyTo(arr: _T1[]): void;
                    CopyTo1(arr: _T1[], offset: number): void;
                    CopyTo2(index: number, target: _T1[], offset: number, count: number): void;
                    GetRange(index: number, count: number): _ResizeArray.ResizeArrayProxy<_T1>;
                    Insert(index: number, items: _T1): void;
                    InsertRange(index: number, items: _WebSharper.seq<_T1>): void;
                    RemoveAt(x: number): void;
                    RemoveRange(index: number, count: number): void;
                    Reverse(): void;
                    Reverse1(index: number, count: number): void;
                    ToArray(): _T1[];
                    get_Count(): number;
                    get_Item(x: number): _T1;
                    set_Item(x: number, v: _T1): void;
                }
            }
            module SetModule {
                var Filter : {
                    <_M1>(f: {
                        (x: _M1): boolean;
                    }, s: _Collections.FSharpSet<_M1>): _Collections.FSharpSet<_M1>;
                };
                var FoldBack : {
                    <_M1, _M2>(f: {
                        (x: _M1): {
                            (x: _M2): _M2;
                        };
                    }, a: _Collections.FSharpSet<_M1>, s: _M2): _M2;
                };
                var Partition : {
                    <_M1>(f: {
                        (x: _M1): boolean;
                    }, a: _Collections.FSharpSet<_M1>): any;
                };
            }
            module MapModule {
                var Exists : {
                    <_M1, _M2>(f: {
                        (x: _M1): {
                            (x: _M2): boolean;
                        };
                    }, m: _Collections.FSharpMap<_M1, _M2>): boolean;
                };
                var Filter : {
                    <_M1, _M2>(f: {
                        (x: _M1): {
                            (x: _M2): boolean;
                        };
                    }, m: _Collections.FSharpMap<_M1, _M2>): _Collections.FSharpMap<_M1, _M2>;
                };
                var FindKey : {
                    <_M1, _M2>(f: {
                        (x: _M1): {
                            (x: _M2): boolean;
                        };
                    }, m: _Collections.FSharpMap<_M1, _M2>): _M1;
                };
                var Fold : {
                    <_M1, _M2, _M3>(f: {
                        (x: _M3): {
                            (x: _M1): {
                                (x: _M2): _M3;
                            };
                        };
                    }, s: _M3, m: _Collections.FSharpMap<_M1, _M2>): _M3;
                };
                var FoldBack : {
                    <_M1, _M2, _M3>(f: {
                        (x: _M1): {
                            (x: _M2): {
                                (x: _M3): _M3;
                            };
                        };
                    }, m: _Collections.FSharpMap<_M1, _M2>, s: _M3): _M3;
                };
                var ForAll : {
                    <_M1, _M2>(f: {
                        (x: _M1): {
                            (x: _M2): boolean;
                        };
                    }, m: _Collections.FSharpMap<_M1, _M2>): boolean;
                };
                var Iterate : {
                    <_M1, _M2>(f: {
                        (x: _M1): {
                            (x: _M2): void;
                        };
                    }, m: _Collections.FSharpMap<_M1, _M2>): void;
                };
                var OfArray : {
                    <_M1, _M2>(a: any[]): _Collections.FSharpMap<_M1, _M2>;
                };
                var Partition : {
                    <_M1, _M2>(f: {
                        (x: _M1): {
                            (x: _M2): boolean;
                        };
                    }, m: _Collections.FSharpMap<_M1, _M2>): any;
                };
                var Pick : {
                    <_M1, _M2, _M3>(f: {
                        (x: _M1): {
                            (x: _M2): _WebSharper.OptionProxy<_M3>;
                        };
                    }, m: _Collections.FSharpMap<_M1, _M2>): _M3;
                };
                var ToSeq : {
                    <_M1, _M2>(m: _Collections.FSharpMap<_M1, _M2>): _WebSharper.seq<any>;
                };
                var TryFind : {
                    <_M1, _M2>(k: _M1, m: _Collections.FSharpMap<_M1, _M2>): _WebSharper.OptionProxy<_M2>;
                };
                var TryFindKey : {
                    <_M1, _M2>(f: {
                        (x: _M1): {
                            (x: _M2): boolean;
                        };
                    }, m: _Collections.FSharpMap<_M1, _M2>): _WebSharper.OptionProxy<_M1>;
                };
                var TryPick : {
                    <_M1, _M2, _M3>(f: {
                        (x: _M1): {
                            (x: _M2): _WebSharper.OptionProxy<_M3>;
                        };
                    }, m: _Collections.FSharpMap<_M1, _M2>): _WebSharper.OptionProxy<_M3>;
                };
                var Map : {
                    <_M1, _M2, _M3>(f: {
                        (x: _M1): {
                            (x: _M2): _M3;
                        };
                    }, m: _Collections.FSharpMap<_M1, _M2>): _Collections.FSharpMap<_M1, _M3>;
                };
            }
            interface Dictionary<_T1, _T2> {
                Add(k: _T1, v: _T2): void;
                Clear(): void;
                ContainsKey(k: _T1): boolean;
                GetEnumerator(): _WebSharper.IEnumeratorProxy<_WebSharper.ObjectProxy>;
                Remove(k: _T1): boolean;
                get_Item(k: _T1): _T2;
                set_Item(k: _T1, v: _T2): void;
            }
            interface FSharpMap<_T1, _T2> {
                Add(k: _T1, v: _T2): _Collections.FSharpMap<_T1, _T2>;
                ContainsKey(k: _T1): boolean;
                Remove(k: _T1): _Collections.FSharpMap<_T1, _T2>;
                TryFind(k: _T1): _WebSharper.OptionProxy<_T2>;
                GetEnumerator(): _WebSharper.IEnumeratorProxy<_WebSharper.KeyValuePairProxy<_T1, _T2>>;
                GetHashCode(): number;
                Equals(other: _WebSharper.ObjectProxy): boolean;
                CompareTo(other: _WebSharper.ObjectProxy): number;
                get_Tree(): any;
                get_Count(): number;
                get_IsEmpty(): boolean;
                get_Item(k: _T1): _T2;
            }
            interface FSharpSet<_T1> {
                add(x: _Collections.FSharpSet<_T1>): _Collections.FSharpSet<_T1>;
                sub(x: _Collections.FSharpSet<_T1>): _Collections.FSharpSet<_T1>;
                Add(x: _T1): _Collections.FSharpSet<_T1>;
                Contains(v: _T1): boolean;
                IsProperSubsetOf(s: _Collections.FSharpSet<_T1>): boolean;
                IsProperSupersetOf(s: _Collections.FSharpSet<_T1>): boolean;
                IsSubsetOf(s: _Collections.FSharpSet<_T1>): boolean;
                IsSupersetOf(s: _Collections.FSharpSet<_T1>): boolean;
                Remove(v: _T1): _Collections.FSharpSet<_T1>;
                GetEnumerator(): _WebSharper.IEnumeratorProxy<_T1>;
                GetHashCode(): number;
                Equals(other: _WebSharper.ObjectProxy): boolean;
                CompareTo(other: _WebSharper.ObjectProxy): number;
                get_Count(): number;
                get_IsEmpty(): boolean;
                get_Tree(): any;
                get_MaximumElement(): _T1;
                get_MinimumElement(): _T1;
            }
        }
    }
    
    import _LinkedList = IntelliFactory.WebSharper.Collections.LinkedList;
    import _WebSharper = IntelliFactory.WebSharper;
    import _ResizeArray = IntelliFactory.WebSharper.Collections.ResizeArray;
    import _Collections = IntelliFactory.WebSharper.Collections;
}
