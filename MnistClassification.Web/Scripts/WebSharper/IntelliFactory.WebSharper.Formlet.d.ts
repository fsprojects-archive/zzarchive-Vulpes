declare module IntelliFactory {
    module WebSharper {
        module Formlet {
            module Body {
                var New : {
                    (el: _Html.Element, l: _WebSharper.OptionProxy<{
                        (): _Html.Element;
                    }>): _Formlet.Body;
                };
            }
            module Layout {
                module Padding {
                    var get_Default : {
                        (): _Layout.Padding;
                    };
                }
                module LabelConfiguration {
                    var get_Default : {
                        (): _Layout.LabelConfiguration;
                    };
                }
                module FormRowConfiguration {
                    var get_Default : {
                        (): _Layout.FormRowConfiguration;
                    };
                }
                interface Align {
                }
                interface VerticalAlign {
                }
                interface FormRowConfiguration {
                    Padding: _WebSharper.OptionProxy<_Layout.Padding>;
                    Color: _WebSharper.OptionProxy<{
                        (x: number): string;
                    }>;
                    Class: _WebSharper.OptionProxy<{
                        (x: number): string;
                    }>;
                    Style: _WebSharper.OptionProxy<{
                        (x: number): string;
                    }>;
                    LabelConfiguration: _WebSharper.OptionProxy<_Layout.LabelConfiguration>;
                }
                interface LabelConfiguration {
                    Align: _Layout.Align;
                    VerticalAlign: _Layout.VerticalAlign;
                    Placement: _Layout.Placement;
                }
                interface Padding {
                    Left: _WebSharper.OptionProxy<number>;
                    Right: _WebSharper.OptionProxy<number>;
                    Top: _WebSharper.OptionProxy<number>;
                    Bottom: _WebSharper.OptionProxy<number>;
                }
                interface Placement {
                }
            }
            module Data {
                interface Formlet<_T1> {
                    Run(f: {
                        (x: _T1): void;
                    }): _Html.IPagelet;
                    Build(): _Base.Form<_Formlet.Body, _T1>;
                    get_Layout(): any;
                    MapResult<_M1>(f: {
                        (x: _Base.Result<_T1>): _Base.Result<_M1>;
                    }): _Base.IFormlet<_Formlet.Body, _T1>;
                    get_Body(): _Dom.Node;
                    Render(): void;
                    BuildInternal: {
                        (): _Base.Form<_Formlet.Body, _T1>;
                    };
                    LayoutInternal: any;
                    ElementInternal: _WebSharper.OptionProxy<_Html.Element>;
                    FormletBase: _Base.FormletProvider<_Formlet.Body>;
                    Utils: any;
                }
                var NewBody : {
                    (arg00: _Html.Element, arg10: _WebSharper.OptionProxy<{
                        (): _Html.Element;
                    }>): _Formlet.Body;
                };
                var UtilsProvider : {
                    (): any;
                };
                var BaseFormlet : {
                    (): _Base.FormletProvider<_Formlet.Body>;
                };
                var PropagateRenderFrom : {
                    <_M1, _M2, _M3>(f1: _Base.IFormlet<_M1, _M2>, f2: _M3): _M3;
                };
                var OfIFormlet : {
                    <_M1>(formlet: _Base.IFormlet<_Formlet.Body, _M1>): _Data.Formlet<_M1>;
                };
                var MkFormlet : {
                    <_M1, _M2, _M3>(f: {
                        (): any;
                    }): _Data.Formlet<_M3>;
                };
                var $ : {
                    <_M1, _M2>(f: _Data.Formlet<{
                        (x: _M1): _M2;
                    }>, x: _Data.Formlet<_M1>): _Data.Formlet<_M2>;
                };
                var RX : {
                    (): _Reactive.IReactive;
                };
                var Layout : {
                    (): _Formlet.LayoutProvider;
                };
                var DefaultLayout : {
                    (): any;
                };
                var Validator : {
                    (): _Base.Validator;
                };
            }
            module Enhance {
                module FormButtonConfiguration {
                    var get_Default : {
                        (): _Enhance.FormButtonConfiguration;
                    };
                }
                module ValidationIconConfiguration {
                    var get_Default : {
                        (): _Enhance.ValidationIconConfiguration;
                    };
                }
                module ValidationFrameConfiguration {
                    var get_Default : {
                        (): _Enhance.ValidationFrameConfiguration;
                    };
                }
                module Padding {
                    var get_Default : {
                        (): _Enhance.Padding;
                    };
                }
                module FormContainerConfiguration {
                    var get_Default : {
                        (): _Enhance.FormContainerConfiguration;
                    };
                }
                module ManyConfiguration {
                    var get_Default : {
                        (): _Enhance.ManyConfiguration;
                    };
                }
                interface FormButtonConfiguration {
                    Label: _WebSharper.OptionProxy<string>;
                    Style: _WebSharper.OptionProxy<string>;
                    Class: _WebSharper.OptionProxy<string>;
                }
                interface ValidationIconConfiguration {
                    ValidIconClass: string;
                    ErrorIconClass: string;
                }
                interface ValidationFrameConfiguration {
                    ValidClass: _WebSharper.OptionProxy<string>;
                    ValidStyle: _WebSharper.OptionProxy<string>;
                    ErrorClass: _WebSharper.OptionProxy<string>;
                    ErrorStyle: _WebSharper.OptionProxy<string>;
                }
                interface Padding {
                    Left: _WebSharper.OptionProxy<number>;
                    Right: _WebSharper.OptionProxy<number>;
                    Top: _WebSharper.OptionProxy<number>;
                    Bottom: _WebSharper.OptionProxy<number>;
                }
                interface FormPart {
                }
                interface FormContainerConfiguration {
                    Header: _WebSharper.OptionProxy<_Enhance.FormPart>;
                    Padding: _Enhance.Padding;
                    Description: _WebSharper.OptionProxy<_Enhance.FormPart>;
                    BackgroundColor: _WebSharper.OptionProxy<string>;
                    BorderColor: _WebSharper.OptionProxy<string>;
                    CssClass: _WebSharper.OptionProxy<string>;
                    Style: _WebSharper.OptionProxy<string>;
                }
                interface ManyConfiguration {
                    AddIconClass: string;
                    RemoveIconClass: string;
                }
                interface JsonPostConfiguration {
                    PostUrl: _WebSharper.OptionProxy<string>;
                    ParameterName: string;
                    EncodingType: _WebSharper.OptionProxy<string>;
                }
                var WithResetFormlet : {
                    <_M1, _M2>(formlet: _Data.Formlet<_M1>, reset: _Data.Formlet<_M2>): _Data.Formlet<_M1>;
                };
                var WithResetAction : {
                    <_M1>(f: {
                        (): boolean;
                    }, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithSubmitFormlet : {
                    <_M1>(formlet: _Data.Formlet<_M1>, submit: {
                        (x: _Base.Result<_M1>): _Data.Formlet<void>;
                    }): _Data.Formlet<_M1>;
                };
                var WithSubmitAndReset : {
                    <_M1, _M2>(formlet: _Data.Formlet<_M1>, submReset: {
                        (x: {
                            (): void;
                        }): {
                            (x: _Base.Result<_M1>): _Data.Formlet<_M2>;
                        };
                    }): _Data.Formlet<_M2>;
                };
                var InputButton : {
                    (conf: _Enhance.FormButtonConfiguration, enabled: boolean): _Data.Formlet<number>;
                };
                var WithCustomSubmitAndResetButtons : {
                    <_M1>(submitConf: _Enhance.FormButtonConfiguration, resetConf: _Enhance.FormButtonConfiguration, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithSubmitAndResetButtons : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithCustomValidationIcon : {
                    <_M1>(vic: _Enhance.ValidationIconConfiguration, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithValidationIcon : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WrapFormlet : {
                    <_M1>(wrapper: {
                        (x: _Control.IObservableProxy<_Base.Result<_M1>>): {
                            (x: _Formlet.Body): _Html.Element;
                        };
                    }, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithCustomValidationFrame : {
                    <_M1>(vc: _Enhance.ValidationFrameConfiguration, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithCustomResetButton : {
                    <_M1>(buttonConf: _Enhance.FormButtonConfiguration, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithResetButton : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithCustomSubmitButton : {
                    <_M1>(buttonConf: _Enhance.FormButtonConfiguration, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithSubmitButton : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithErrorSummary : {
                    <_M1>(label: string, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithValidationFrame : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithErrorFormlet : {
                    <_M1, _M2>(f: {
                        (x: _List.T<string>): _Data.Formlet<_M1>;
                    }, formlet: _Data.Formlet<_M2>): _Data.Formlet<_M2>;
                };
                var WithLabel : {
                    <_M1>(labelGen: {
                        (): _Html.Element;
                    }, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithLabelConfiguration : {
                    <_M1>(lc: _Layout.LabelConfiguration, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithLabelAndInfo : {
                    <_M1>(label: string, info: string, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithTextLabel : {
                    <_M1>(label: string, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithLabelAbove : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithLabelLeft : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithCustomFormContainer : {
                    <_M1>(fc: _Enhance.FormContainerConfiguration, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithFormContainer : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithCssClass : {
                    <_M1>(css: string, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithLegend : {
                    <_M1>(label: string, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithRowConfiguration : {
                    <_M1>(rc: _Layout.FormRowConfiguration, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var Cancel : {
                    <_M1>(formlet: _Data.Formlet<_M1>, isCancel: {
                        (x: _M1): boolean;
                    }): _Data.Formlet<_M1>;
                };
                var Replace : {
                    <_M1, _M2>(formlet: _Data.Formlet<_M1>, f: {
                        (x: _M1): _Data.Formlet<_M2>;
                    }): _Data.Formlet<_M2>;
                };
                var Deletable : {
                    <_M1>(formlet: _Data.Formlet<_WebSharper.OptionProxy<_M1>>): _Data.Formlet<_WebSharper.OptionProxy<_M1>>;
                };
                var Many_ : {
                    <_M1, _M2>(add: _Data.Formlet<_M1>, f: {
                        (x: _M1): _Data.Formlet<_WebSharper.OptionProxy<_M2>>;
                    }): _Data.Formlet<_WebSharper.seq<_M2>>;
                };
                var CustomMany : {
                    <_M1>(config: _Enhance.ManyConfiguration, formlet: _Data.Formlet<_M1>): _Data.Formlet<_List.T<_M1>>;
                };
                var Many : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_List.T<_M1>>;
                };
                var WithJsonPost : {
                    <_M1>(conf: any, formlet: _Data.Formlet<_M1>): _Html.Element;
                };
            }
            module Controls {
                var SelectControl : {
                    <_M1>(readOnly: boolean, def: number, vls: _List.T<any>): _Data.Formlet<_M1>;
                };
                var Select : {
                    <_M1>(def: number, vls: _List.T<any>): _Data.Formlet<_M1>;
                };
                var ReadOnlySelect : {
                    <_M1>(def: number, vls: _List.T<any>): _Data.Formlet<_M1>;
                };
                var InputControl : {
                    (value: string, f: {
                        (x: _Reactive.HotStream<_Base.Result<string>>): _Html.Element;
                    }): _Data.Formlet<string>;
                };
                var OnTextChange : {
                    (f: {
                        (): void;
                    }, control: _Html.Element): void;
                };
                var TextAreaControl : {
                    (readOnly: boolean, value: string): _Data.Formlet<string>;
                };
                var TextArea : {
                    (value: string): _Data.Formlet<string>;
                };
                var ReadOnlyTextArea : {
                    (value: string): _Data.Formlet<string>;
                };
                var InputField : {
                    (readOnly: boolean, typ: string, cls: string, value: string): _Data.Formlet<string>;
                };
                var CheckboxControl : {
                    (readOnly: boolean, def: boolean): _Data.Formlet<boolean>;
                };
                var Checkbox : {
                    (def: boolean): _Data.Formlet<boolean>;
                };
                var ReadOnlyCheckbox : {
                    (def: boolean): _Data.Formlet<boolean>;
                };
                var CheckboxGroupControl : {
                    <_M1>(readOnly: boolean, values: _List.T<any>): _Data.Formlet<_List.T<_M1>>;
                };
                var CheckboxGroup : {
                    <_M1>(values: _List.T<any>): _Data.Formlet<_List.T<_M1>>;
                };
                var RadioButtonGroupControl : {
                    <_M1>(readOnly: boolean, def: _WebSharper.OptionProxy<number>, values: _List.T<any>): _Data.Formlet<_M1>;
                };
                var RadioButtonGroup : {
                    <_M1>(def: _WebSharper.OptionProxy<number>, values: _List.T<any>): _Data.Formlet<_M1>;
                };
                var ReadOnlyRadioButtonGroup : {
                    <_M1>(def: _WebSharper.OptionProxy<number>, values: _List.T<any>): _Data.Formlet<_M1>;
                };
                var Password : {
                    (value: string): _Data.Formlet<string>;
                };
                var Input : {
                    (value: string): _Data.Formlet<string>;
                };
                var ReadOnlyInput : {
                    (value: string): _Data.Formlet<string>;
                };
                var ElementButton : {
                    (genElem: {
                        (): _Html.Element;
                    }): _Data.Formlet<number>;
                };
                var Button : {
                    (label: string): _Data.Formlet<number>;
                };
            }
            module Formlet {
                var BuildFormlet : {
                    <_M1, _M2, _M3>(f: {
                        (): any;
                    }): _Data.Formlet<_M3>;
                };
                var New : {
                    <_M1>(f: {
                        (): _Base.Form<_Formlet.Body, _M1>;
                    }): _Data.Formlet<_M1>;
                };
                var WithLayoutOrDefault : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var Return : {
                    <_M1>(x: _M1): _Data.Formlet<_M1>;
                };
                var WithCancelation : {
                    <_M1>(formlet: _Data.Formlet<_M1>, c: _Data.Formlet<void>): _Data.Formlet<_WebSharper.OptionProxy<_M1>>;
                };
                var InitWith : {
                    <_M1>(value: _M1, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var InitWithFailure : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var Horizontal : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var Vertical : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var Flowlet : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var ReplaceFirstWithFailure : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var Never : {
                    <_M1>(): _Data.Formlet<_M1>;
                };
                var Empty : {
                    <_M1>(): _Data.Formlet<_M1>;
                };
                var ReturnEmpty : {
                    <_M1>(x: _M1): _Data.Formlet<_M1>;
                };
                var BuildForm : {
                    <_M1>(f: _Data.Formlet<_M1>): _Base.Form<_Formlet.Body, _M1>;
                };
                var Deletable : {
                    <_M1>(formlet: _Data.Formlet<_WebSharper.OptionProxy<_M1>>): _Data.Formlet<_WebSharper.OptionProxy<_M1>>;
                };
                var FailWith : {
                    <_M1>(fs: _List.T<string>): _Data.Formlet<_M1>;
                };
                var Map : {
                    <_M1, _M2>(f: {
                        (x: _M1): _M2;
                    }, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M2>;
                };
                var MapBody : {
                    <_M1>(f: {
                        (x: _Formlet.Body): _Formlet.Body;
                    }, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var MapResult : {
                    <_M1, _M2>(f: {
                        (x: _Base.Result<_M1>): _Base.Result<_M2>;
                    }, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M2>;
                };
                var Delay : {
                    <_M1>(f: {
                        (): _Data.Formlet<_M1>;
                    }): _Data.Formlet<_M1>;
                };
                var Bind : {
                    <_M1, _M2>(fl: _Data.Formlet<_M1>, f: {
                        (x: _M1): _Data.Formlet<_M2>;
                    }): _Data.Formlet<_M2>;
                };
                var Replace : {
                    <_M1, _M2>(formlet: _Data.Formlet<_M1>, f: {
                        (x: _M1): _Data.Formlet<_M2>;
                    }): _Data.Formlet<_M2>;
                };
                var Join : {
                    <_M1>(formlet: _Data.Formlet<_Data.Formlet<_M1>>): _Data.Formlet<_M1>;
                };
                var Switch : {
                    <_M1>(formlet: _Data.Formlet<_Data.Formlet<_M1>>): _Data.Formlet<_M1>;
                };
                var FlipBody : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var SelectMany : {
                    <_M1>(formlet: _Data.Formlet<_Data.Formlet<_M1>>): _Data.Formlet<_List.T<_M1>>;
                };
                var LiftResult : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_Base.Result<_M1>>;
                };
                var Sequence : {
                    <_M1>(fs: _WebSharper.seq<_Data.Formlet<_M1>>): _Data.Formlet<_List.T<_M1>>;
                };
                var WithLayout : {
                    <_M1>(l: any, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithNotification : {
                    <_M1>(c: {
                        (x: _WebSharper.ObjectProxy): void;
                    }, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var WithNotificationChannel : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<any>;
                };
                var ApplyLayout : {
                    <_M1>(formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var MapElement : {
                    <_M1>(f: {
                        (x: _Html.Element): _Html.Element;
                    }, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var OfElement : {
                    (genElem: {
                        (): _Html.Element;
                    }): _Data.Formlet<void>;
                };
                var WithLabel : {
                    <_M1>(label: _WebSharper.OptionProxy<{
                        (): _Html.Element;
                    }>, formlet: _Data.Formlet<_M1>): _Data.Formlet<_M1>;
                };
                var Run : {
                    <_M1>(f: {
                        (x: _M1): void;
                    }, formlet: _Data.Formlet<_M1>): _Html.IPagelet;
                };
                var BindWith : {
                    <_M1, _M2>(compose: {
                        (x: _Formlet.Body): {
                            (x: _Formlet.Body): _Formlet.Body;
                        };
                    }, formlet: _Data.Formlet<_M1>, f: {
                        (x: _M1): _Data.Formlet<_M2>;
                    }): _Data.Formlet<_M2>;
                };
                var Render : {
                    (formlet: _Data.Formlet<void>): _Html.IPagelet;
                };
                var Choose : {
                    <_M1>(fs: _WebSharper.seq<_Data.Formlet<_M1>>): _Data.Formlet<_M1>;
                };
                var Do : {
                    (): _Formlet.FormletBuilder;
                };
            }
            module CssConstants {
                var InputTextClass : {
                    (): string;
                };
            }
            interface Body {
                Element: _Html.Element;
                Label: _WebSharper.OptionProxy<{
                    (): _Html.Element;
                }>;
            }
            interface LayoutProvider {
                HorizontalAlignElem(align: _Layout.Align, el: _Html.Element): _Html.Element;
                VerticalAlignedTD(valign: _Layout.VerticalAlign, elem: _Html.Element): _Html.Element;
                MakeRow(rowConfig: _Layout.FormRowConfiguration, rowIndex: number, body: _Formlet.Body): _Html.Element;
                MakeLayout(lm: {
                    (): any;
                }): any;
                RowLayout(rowConfig: _Layout.FormRowConfiguration): any;
                ColumnLayout(rowConfig: _Layout.FormRowConfiguration): any;
                LabelLayout(lc: _Layout.LabelConfiguration): any;
                get_Flowlet(): any;
                get_Vertical(): any;
                get_Horizontal(): any;
            }
            interface FormletBuilder {
                Return<_M1>(x: _M1): _Data.Formlet<_M1>;
                Bind<_M1, _M2>(formlet: _Data.Formlet<_M1>, f: {
                    (x: _M1): _Data.Formlet<_M2>;
                }): _Data.Formlet<_M2>;
                Delay<_M1>(f: {
                    (): _Data.Formlet<_M1>;
                }): _Data.Formlet<_M1>;
                ReturnFrom<_M1>(f: _Base.IFormlet<_Formlet.Body, _M1>): _Data.Formlet<_M1>;
            }
        }
    }
    
    import _Html = IntelliFactory.WebSharper.Html;
    import _WebSharper = IntelliFactory.WebSharper;
    import _Formlet = IntelliFactory.WebSharper.Formlet;
    import _Layout = IntelliFactory.WebSharper.Formlet.Layout;
    import _Base = IntelliFactory.Formlet.Base;
    import _Dom = IntelliFactory.WebSharper.Dom;
    import _Data = IntelliFactory.WebSharper.Formlet.Data;
    import _Reactive = IntelliFactory.Reactive;
    import _Enhance = IntelliFactory.WebSharper.Formlet.Enhance;
    import _Control = IntelliFactory.WebSharper.Control;
    import _List = IntelliFactory.WebSharper.List;
}
