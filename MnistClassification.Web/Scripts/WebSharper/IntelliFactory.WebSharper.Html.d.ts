declare module IntelliFactory {
    module WebSharper {
        module Html {
            module Element {
                var New : {
                    (html: _Interfaces.IHtmlProvider, name: string): _Html.Element;
                };
            }
            module Interfaces {
                interface IHtmlProvider {
                    CreateTextNode(x0: string): _Dom.Text;
                    CreateElement(x0: string): _Dom.Element;
                    SetAttribute(x0: _Dom.Node, x1: string, x2: string): void;
                    AppendAttribute(x0: _Dom.Node, x1: _Dom.Attr): void;
                    RemoveAttribute(x0: _Dom.Node, x1: string): void;
                    GetAttribute(x0: _Dom.Node, x1: string): string;
                    HasAttribute(x0: _Dom.Node, x1: string): boolean;
                    CreateAttribute(x0: string): _Dom.Attr;
                    GetProperty<_M1>(x0: _Dom.Node, x1: string): _M1;
                    SetProperty<_M1>(x0: _Dom.Node, x1: string, x2: _M1): void;
                    AppendNode(x0: _Dom.Node, x1: _Dom.Node): void;
                    Clear(x0: _Dom.Node): void;
                    Remove(x0: _Dom.Node): void;
                    SetText(x0: _Dom.Node, x1: string): void;
                    GetText(x0: _Dom.Node): string;
                    SetHtml(x0: _Dom.Node, x1: string): void;
                    GetHtml(x0: _Dom.Node): string;
                    SetValue(x0: _Dom.Node, x1: string): void;
                    GetValue(x0: _Dom.Node): string;
                    SetStyle(x0: _Dom.Node, x1: string): void;
                    SetCss(x0: _Dom.Node, x1: string, x2: string): void;
                    AddClass(x0: _Dom.Node, x1: string): void;
                    RemoveClass(x0: _Dom.Node, x1: string): void;
                    OnLoad(x0: _Dom.Node, x1: {
                        (): void;
                    }): void;
                    OnDocumentReady(x0: {
                        (): void;
                    }): void;
                }
            }
            module Activator {
                interface IControl {
                    get_Body(): _Html.IPagelet;
                }
                var Activate : {
                    (): void;
                };
            }
            module EventsPervasives {
                var Events : {
                    (): _Events.IEventSupport;
                };
            }
            module Events {
                interface IEventSupport {
                    OnClick<_M1>(x0: {
                        (x: _M1): {
                            (x: any): void;
                        };
                    }, x1: _M1): void;
                    OnDoubleClick<_M1>(x0: {
                        (x: _M1): {
                            (x: any): void;
                        };
                    }, x1: _M1): void;
                    OnMouseDown<_M1>(x0: {
                        (x: _M1): {
                            (x: any): void;
                        };
                    }, x1: _M1): void;
                    OnMouseEnter<_M1>(x0: {
                        (x: _M1): {
                            (x: any): void;
                        };
                    }, x1: _M1): void;
                    OnMouseLeave<_M1>(x0: {
                        (x: _M1): {
                            (x: any): void;
                        };
                    }, x1: _M1): void;
                    OnMouseMove<_M1>(x0: {
                        (x: _M1): {
                            (x: any): void;
                        };
                    }, x1: _M1): void;
                    OnMouseOut<_M1>(x0: {
                        (x: _M1): {
                            (x: any): void;
                        };
                    }, x1: _M1): void;
                    OnMouseUp<_M1>(x0: {
                        (x: _M1): {
                            (x: any): void;
                        };
                    }, x1: _M1): void;
                    OnKeyDown<_M1>(x0: {
                        (x: _M1): {
                            (x: any): void;
                        };
                    }, x1: _M1): void;
                    OnKeyPress<_M1>(x0: {
                        (x: _M1): {
                            (x: any): void;
                        };
                    }, x1: _M1): void;
                    OnKeyUp<_M1>(x0: {
                        (x: _M1): {
                            (x: any): void;
                        };
                    }, x1: _M1): void;
                    OnBlur<_M1>(x0: {
                        (x: _M1): void;
                    }, x1: _M1): void;
                    OnChange<_M1>(x0: {
                        (x: _M1): void;
                    }, x1: _M1): void;
                    OnFocus<_M1>(x0: {
                        (x: _M1): void;
                    }, x1: _M1): void;
                    OnError<_M1>(x0: {
                        (x: _M1): void;
                    }, x1: _M1): void;
                    OnLoad<_M1>(x0: {
                        (x: _M1): void;
                    }, x1: _M1): void;
                    OnUnLoad<_M1>(x0: {
                        (x: _M1): void;
                    }, x1: _M1): void;
                    OnResize<_M1>(x0: {
                        (x: _M1): void;
                    }, x1: _M1): void;
                    OnScroll<_M1>(x0: {
                        (x: _M1): void;
                    }, x1: _M1): void;
                    OnSelect<_M1>(x0: {
                        (x: _M1): void;
                    }, x1: _M1): void;
                    OnSubmit<_M1>(x0: {
                        (x: _M1): void;
                    }, x1: _M1): void;
                }
                interface MouseEvent {
                    X: number;
                    Y: number;
                }
                interface CharacterCode {
                    CharacterCode: number;
                }
                interface KeyCode {
                    KeyCode: number;
                }
            }
            module Default {
                module HTML5 {
                    var Tags : {
                        (): _Html.Html5TagBuilder;
                    };
                    var Attr : {
                        (): _Html.Html5AttributeBuilder;
                    };
                }
                var OnLoad : {
                    (init: {
                        (): void;
                    }): void;
                };
                var Text : {
                    (x: string): _Html.IPagelet;
                };
                var A : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var B : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Body : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Br : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Button : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Code : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Div : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Em : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Form : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var H1 : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var H2 : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var H3 : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var H4 : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Head : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Hr : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var I : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var IFrame : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Img : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Input : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var LI : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var OL : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var P : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Pre : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Script : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Select : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Span : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var Table : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var TBody : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var TD : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var TextArea : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var TFoot : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var TH : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var THead : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var TR : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var UL : {
                    <_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
                };
                var NewAttr : {
                    (x: string): {
                        (x: string): _Html.IPagelet;
                    };
                };
                var Action : {
                    (x: string): _Html.IPagelet;
                };
                var Align : {
                    (x: string): _Html.IPagelet;
                };
                var Alt : {
                    (x: string): _Html.IPagelet;
                };
                var HRef : {
                    (x: string): _Html.IPagelet;
                };
                var Height : {
                    (x: string): _Html.IPagelet;
                };
                var Id : {
                    (x: string): _Html.IPagelet;
                };
                var Name : {
                    (x: string): _Html.IPagelet;
                };
                var RowSpan : {
                    (x: string): _Html.IPagelet;
                };
                var Selected : {
                    (x: string): _Html.IPagelet;
                };
                var Src : {
                    (x: string): _Html.IPagelet;
                };
                var VAlign : {
                    (x: string): _Html.IPagelet;
                };
                var Width : {
                    (x: string): _Html.IPagelet;
                };
                var Tags : {
                    (): _Html.TagBuilder;
                };
                var Deprecated : {
                    (): _Html.DeprecatedTagBuilder;
                };
                var Attr : {
                    (): _Html.AttributeBuilder;
                };
            }
            module Operators {
                var add : {
                    <_M1>(el: _Html.Element, inner: _WebSharper.seq<_M1>): _Html.Element;
                };
                var OnAfterRender : {
                    <_M1>(f: {
                        (x: _M1): void;
                    }, w: _M1): void;
                };
                var OnBeforeRender : {
                    <_M1>(f: {
                        (x: _M1): void;
                    }, w: _M1): void;
                };
            }
            module PageletExtensions {
                var IPagelet_AppendTo : {
                    (p: _Html.IPagelet, targetId: string): void;
                };
            }
            interface IPagelet {
                Render(): void;
                get_Body(): _Dom.Node;
            }
            interface Element {
                OnLoad(f: {
                    (): void;
                }): void;
                AppendI(pl: _Html.IPagelet): void;
                AppendN(node: _Dom.Node): void;
                Render(): void;
                get_Body(): _Dom.Node;
                get_Text(): string;
                set_Text(x: string): void;
                get_Html(): string;
                set_Html(x: string): void;
                get_Value(): string;
                set_Value(x: string): void;
                get_Id(): string;
                get_HtmlProvider(): _Interfaces.IHtmlProvider;
                get_Item(name: string): string;
                set_Item(name: string, value: string): void;
            }
            interface Html5TagBuilder {
                NewTag<_M1>(name: string, children: _WebSharper.seq<_M1>): _Html.Element;
            }
            interface DeprecatedTagBuilder {
                NewTag<_M1>(name: string, children: _WebSharper.seq<_M1>): _Html.Element;
            }
            interface TagBuilder {
                NewTag<_M1>(name: string, children: _WebSharper.seq<_M1>): _Html.Element;
                text(data: string): _Html.IPagelet;
                Div<_M1>(x: _WebSharper.seq<_M1>): _Html.Element;
            }
            interface Html5AttributeBuilder {
                NewAttr(name: string, value: string): _Html.IPagelet;
            }
            interface DeprecatedAttributeBuilder {
                NewAttr(name: string, value: string): _Html.IPagelet;
            }
            interface AttributeBuilder {
                NewAttr(name: string, value: string): _Html.IPagelet;
                Class(x: string): _Html.IPagelet;
                get_CheckBox(): _Html.IPagelet;
                get_Hidden(): _Html.IPagelet;
                get_Radio(): _Html.IPagelet;
                get_Reset(): _Html.IPagelet;
                get_Submit(): _Html.IPagelet;
                get_Password(): _Html.IPagelet;
                get_TextField(): _Html.IPagelet;
            }
        }
    }
    
    import _Interfaces = IntelliFactory.WebSharper.Html.Interfaces;
    import _Html = IntelliFactory.WebSharper.Html;
    import _Dom = IntelliFactory.WebSharper.Dom;
    import _Events = IntelliFactory.WebSharper.Html.Events;
    import _WebSharper = IntelliFactory.WebSharper;
}
