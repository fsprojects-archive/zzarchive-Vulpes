declare module IntelliFactory {
    module WebSharper {
        module Sitelets {
            module Sitelet1 {
                interface Filter<_T1> {
                    VerifyUser: {
                        (x: string): boolean;
                    };
                    LoginRedirect: {
                        (x: _T1): _T1;
                    };
                }
            }
            module Content1 {
                module Template {
                    interface LoadFrequency {
                    }
                }
            }
            module Http {
                interface Method {
                }
                interface Version {
                    Version: number;
                }
                interface Header {
                    name: string;
                    value: string;
                }
                interface Request {
                    Method: _Http.Method;
                    Uri: any;
                    Headers: _WebSharper.seq<any>;
                    Post: any;
                    Get: any;
                    Cookies: any;
                    ServerVariables: any;
                    Body: any;
                    Files: _WebSharper.seq<any>;
                }
                interface Status {
                    SCode: number;
                    SMessage: string;
                }
                interface Response {
                    Status: any;
                    Headers: _WebSharper.seq<any>;
                    WriteBody: {
                        (x: any): void;
                    };
                }
            }
            module UrlEncoding {
                interface NoFormatError {
                }
                interface Format<_T1> {
                    read: {
                        (x: string): _WebSharper.OptionProxy<_WebSharper.ObjectProxy>;
                    };
                    show: {
                        (x: _WebSharper.ObjectProxy): _WebSharper.OptionProxy<string>;
                    };
                }
            }
            interface Page {
                Doctype: _WebSharper.OptionProxy<string>;
                Title: _WebSharper.OptionProxy<string>;
                Renderer: {
                    (x: _WebSharper.OptionProxy<string>): {
                        (x: _WebSharper.OptionProxy<string>): {
                            (x: {
                                (x: any): void;
                            }): {
                                (x: {
                                    (x: any): void;
                                }): {
                                    (x: any): void;
                                };
                            };
                        };
                    };
                };
                Head: _WebSharper.seq<_Html.Element<void>>;
                Body: _WebSharper.seq<_Html.Element<_Web.Control>>;
            }
            interface Router<_T1> {
                StaticRoutes: any;
                StaticLinks: any;
                DynamicRoute: {
                    (x: any): _WebSharper.OptionProxy<_T1>;
                };
                DynamicLink: {
                    (x: _T1): _WebSharper.OptionProxy<any>;
                };
            }
            interface Context<_T1> {
                ApplicationPath: string;
                Link: {
                    (x: _T1): string;
                };
                Json: any;
                Metadata: any;
                ResolveUrl: {
                    (x: string): string;
                };
                ResourceContext: any;
                Request: any;
                RootFolder: string;
            }
            interface Content<_T1> {
            }
            interface Controller<_T1> {
                Handle: {
                    (x: _T1): _Sitelets.Content<_T1>;
                };
            }
            interface Sitelet<_T1> {
                Router: any;
                Controller: any;
            }
            interface IWebsite<_T1> {
                get_Actions(): _List.T<_T1>;
                get_Sitelet(): any;
            }
            interface SinglePageAction {
            }
            interface SinglePageWebsite {
            }
            interface IHostedWebsite<_T1> {
                Build(x0: any): _Sitelets.IWebsite<_T1>;
            }
            interface HttpHandler {
            }
            interface HttpModule {
            }
            interface Plugin {
            }
        }
    }
    
    import _Http = IntelliFactory.WebSharper.Sitelets.Http;
    import _WebSharper = IntelliFactory.WebSharper;
    import _Html = IntelliFactory.Html.Html;
    import _Web = IntelliFactory.WebSharper.Web;
    import _Sitelets = IntelliFactory.WebSharper.Sitelets;
    import _List = IntelliFactory.WebSharper.List;
}
