declare module IntelliFactory {
    module Formlet {
        module Base {
            module Tree {
                interface Edit<_T1> {
                    GetEnumerator(): _WebSharper.IEnumeratorProxy1;
                    GetEnumerator1(): _WebSharper.IEnumeratorProxy<_T1>;
                    get_Sequence(): _WebSharper.seq<_T1>;
                }
                interface Tree<_T1> {
                    Map<_M1>(f: {
                        (x: _T1): _M1;
                    }): _Tree.Tree<_M1>;
                    GetEnumerator(): _WebSharper.IEnumeratorProxy1;
                    GetEnumerator1(): _WebSharper.IEnumeratorProxy<_T1>;
                    get_Sequence(): _WebSharper.seq<_T1>;
                }
                var ShowEdit : {
                    <_M1>(edit: _Tree.Edit<_M1>): string;
                };
                var Count : {
                    <_M1>(t: _Tree.Tree<_M1>): number;
                };
                var Range : {
                    <_M1>(edit: _Tree.Edit<_M1>, input: _Tree.Tree<_M1>): any;
                };
                var FromSequence : {
                    <_M1>(vs: _WebSharper.seq<_M1>): _Tree.Tree<_M1>;
                };
                var ReplacedTree : {
                    <_M1>(edit: _Tree.Edit<_M1>, input: _Tree.Tree<_M1>): _Tree.Tree<_M1>;
                };
                var Apply : {
                    <_M1>(edit: _Tree.Edit<_M1>, input: _Tree.Tree<_M1>): _Tree.Tree<_M1>;
                };
                var Set : {
                    <_M1>(value: _M1): _Tree.Edit<_M1>;
                };
                var Transform : {
                    <_M1, _M2>(f: {
                        (x: _Tree.Tree<_M1>): _Tree.Tree<_M2>;
                    }, edit: _Tree.Edit<_M1>): _Tree.Edit<_M2>;
                };
                var Delete : {
                    <_M1>(): _Tree.Edit<_M1>;
                };
                var FlipEdit : {
                    <_M1>(edit: _Tree.Edit<_M1>): _Tree.Edit<_M1>;
                };
                var DeepFlipEdit : {
                    <_M1>(edit: _Tree.Edit<_M1>): _Tree.Edit<_M1>;
                };
            }
            module Result {
                var Join : {
                    (res: _Base.Result<_Base.Result<any>>): _Base.Result<any>;
                };
                var Apply : {
                    <_M1>(f: _Base.Result<{
                        (x: any): _M1;
                    }>, r: _Base.Result<any>): _Base.Result<_M1>;
                };
                var OfOption : {
                    (o: _WebSharper.OptionProxy<any>): _Base.Result<any>;
                };
                var Map : {
                    <_M1>(f: {
                        (x: any): _M1;
                    }, res: _Base.Result<any>): _Base.Result<_M1>;
                };
                var Sequence : {
                    (rs: _WebSharper.seq<_Base.Result<any>>): _Base.Result<_List.T<any>>;
                };
            }
            interface Layout<_T1> {
                Apply: {
                    (x: _Control.IObservableProxy<_Tree.Edit<_T1>>): _WebSharper.OptionProxy<any>;
                };
            }
            interface Container<_T1> {
                Body: _T1;
                SyncRoot: _WebSharper.ObjectProxy;
                Insert: {
                    (x: number): {
                        (x: _T1): void;
                    };
                };
                Remove: {
                    (x: _WebSharper.seq<_T1>): void;
                };
            }
            interface Reactive {
                Reactive: _Reactive.IReactive;
            }
            interface LayoutUtils {
                Default<_M1>(): any;
                Delay<_M1>(f: {
                    (): any;
                }): any;
                New<_M1>(container: {
                    (): any;
                }): any;
            }
            interface Result<_T1> {
            }
            interface Form<_T1, _T2> {
                Dispose(): void;
                Body: _Control.IObservableProxy<_Tree.Edit<_T1>>;
                Dispose1: {
                    (): void;
                };
                Notify: {
                    (x: _WebSharper.ObjectProxy): void;
                };
                State: _Control.IObservableProxy<_Base.Result<_T2>>;
            }
            interface IFormlet<_T1, _T2> {
                Build(): _Base.Form<_T1, _T2>;
                MapResult<_M1>(x0: {
                    (x: _Base.Result<_T2>): _Base.Result<_M1>;
                }): _Base.IFormlet<_T1, _T2>;
                get_Layout(): any;
            }
            interface Utils<_T1> {
                Reactive: _Reactive.IReactive;
                DefaultLayout: any;
            }
            interface FormletProvider<_T1> {
                BuildForm<_M1>(formlet: _Base.IFormlet<_T1, _M1>): _Base.Form<_T1, _M1>;
                New<_M1>(build: {
                    (): _Base.Form<_T1, _M1>;
                }): _Base.IFormlet<_T1, _M1>;
                FromState<_M1>(state: _Control.IObservableProxy<_Base.Result<_M1>>): _Base.IFormlet<_T1, _M1>;
                WithLayout<_M1>(layout: any, formlet: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, _M1>;
                InitWith<_M1>(value: _M1, formlet: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, _M1>;
                ReplaceFirstWithFailure<_M1>(formlet: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, _M1>;
                InitWithFailure<_M1>(formlet: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, _M1>;
                ApplyLayout<_M1>(formlet: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, _M1>;
                AppendLayout<_M1>(layout: any, formlet: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, _M1>;
                MapBody<_M1>(f: {
                    (x: _T1): _T1;
                }, formlet: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, _M1>;
                WithLayoutOrDefault<_M1>(formlet: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, _M1>;
                MapResult<_M1, _M2>(f: {
                    (x: _Base.Result<_M1>): _Base.Result<_M2>;
                }, formlet: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, _M2>;
                Map<_M1, _M2>(f: {
                    (x: _M1): _M2;
                }, formlet: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, _M2>;
                Apply<_M1, _M2>(f: _Base.IFormlet<_T1, {
                    (x: _M1): _M2;
                }>, x: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, _M2>;
                Return<_M1>(x: _M1): _Base.IFormlet<_T1, _M1>;
                Fail<_M1>(fs: _List.T<string>): _Base.Form<_T1, _M1>;
                FailWith<_M1>(fs: _List.T<string>): _Base.IFormlet<_T1, _M1>;
                ReturnEmpty<_M1>(x: _M1): _Base.IFormlet<_T1, _M1>;
                Never<_M1>(): _Base.IFormlet<_T1, _M1>;
                Empty<_M1>(): _Base.IFormlet<_T1, _M1>;
                EmptyForm<_M1>(): _Base.Form<_T1, _M1>;
                Join<_M1>(formlet: _Base.IFormlet<_T1, _Base.IFormlet<_T1, _M1>>): _Base.IFormlet<_T1, _M1>;
                Switch<_M1>(formlet: _Base.IFormlet<_T1, _Base.IFormlet<_T1, _M1>>): _Base.IFormlet<_T1, _M1>;
                FlipBody<_M1>(formlet: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, _M1>;
                SelectMany<_M1>(formlet: _Base.IFormlet<_T1, _Base.IFormlet<_T1, _M1>>): _Base.IFormlet<_T1, _List.T<_M1>>;
                WithNotificationChannel<_M1>(formlet: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, any>;
                Replace<_M1, _M2>(formlet: _Base.IFormlet<_T1, _M1>, f: {
                    (x: _M1): _Base.IFormlet<_T1, _M2>;
                }): _Base.IFormlet<_T1, _M2>;
                Deletable<_M1>(formlet: _Base.IFormlet<_T1, _WebSharper.OptionProxy<_M1>>): _Base.IFormlet<_T1, _WebSharper.OptionProxy<_M1>>;
                WithCancelation<_M1>(formlet: _Base.IFormlet<_T1, _M1>, cancelFormlet: _Base.IFormlet<_T1, void>): _Base.IFormlet<_T1, _WebSharper.OptionProxy<_M1>>;
                Bind<_M1, _M2>(formlet: _Base.IFormlet<_T1, _M1>, f: {
                    (x: _M1): _Base.IFormlet<_T1, _M2>;
                }): _Base.IFormlet<_T1, _M2>;
                Delay<_M1>(f: {
                    (): _Base.IFormlet<_T1, _M1>;
                }): _Base.IFormlet<_T1, _M1>;
                Sequence<_M1, _M2>(fs: _WebSharper.seq<_Base.IFormlet<_T1, _M2>>): _Base.IFormlet<_T1, _List.T<_M2>>;
                LiftResult<_M1>(formlet: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, _Base.Result<_M1>>;
                WithNotification<_M1>(notify: {
                    (x: _WebSharper.ObjectProxy): void;
                }, formlet: _Base.IFormlet<_T1, _M1>): _Base.IFormlet<_T1, _M1>;
                BindWith<_M1, _M2>(hF: {
                    (x: _T1): {
                        (x: _T1): _T1;
                    };
                }, formlet: _Base.IFormlet<_T1, _M1>, f: {
                    (x: _M1): _Base.IFormlet<_T1, _M2>;
                }): _Base.IFormlet<_T1, _M2>;
            }
            interface FormletBuilder<_T1> {
                Return<_M1>(x: _M1): _Base.IFormlet<_T1, _M1>;
                Bind<_M1, _M2>(x: _Base.IFormlet<_T1, _M1>, f: {
                    (x: _M1): _Base.IFormlet<_T1, _M2>;
                }): _Base.IFormlet<_T1, _M2>;
                Delay<_M1>(f: {
                    (): _Base.IFormlet<_T1, _M1>;
                }): _Base.IFormlet<_T1, _M1>;
                ReturnFrom<_M1>(f: _M1): _M1;
            }
            interface IValidatorProvider {
                Matches(x0: string, x1: string): boolean;
            }
            interface Validator {
                Validate<_M1, _M2, _M3>(f: {
                    (x: _M2): boolean;
                }, msg: string, flet: _M3): _M3;
                Is<_M1, _M2, _M3>(f: {
                    (x: _M1): boolean;
                }, m: string, flet: _M2): _M2;
                IsNotEmpty<_M1, _M2>(msg: string, flet: _M2): _M2;
                IsRegexMatch<_M1, _M2>(regex: string, msg: string, flet: _M1): _M1;
                IsEmail<_M1, _M2>(msg: string): {
                    (x: _M1): _M1;
                };
                IsInt<_M1, _M2>(msg: string): {
                    (x: _M1): _M1;
                };
                IsFloat<_M1, _M2>(msg: string): {
                    (x: _M1): _M1;
                };
                IsTrue<_M1, _M2>(msg: string, flet: _M1): _M1;
                IsGreaterThan<_M1, _M2, _M3>(min: _M1, msg: string, flet: _M2): _M2;
                IsLessThan<_M1, _M2, _M3>(max: _M1, msg: string, flet: _M2): _M2;
                IsEqual<_M1, _M2, _M3>(value: _M1, msg: string, flet: _M2): _M2;
                IsNotEqual<_M1, _M2, _M3>(value: _M1, msg: string, flet: _M2): _M2;
            }
        }
    }
    
    import _WebSharper = IntelliFactory.WebSharper;
    import _Tree = IntelliFactory.Formlet.Base.Tree;
    import _Base = IntelliFactory.Formlet.Base;
    import _List = IntelliFactory.WebSharper.List;
    import _Control = IntelliFactory.WebSharper.Control;
    import _Reactive = IntelliFactory.Reactive;
}
