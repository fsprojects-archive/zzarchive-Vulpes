declare module MnistClassification {
    module Web {
        module Skin {
            interface Page {
                Title: string;
                Body: _List.T<_Html.Element<_Web.Control>>;
            }
        }
        module Controls {
            interface EntryPoint {
                get_Body(): _Html1.IPagelet;
            }
        }
        module Client {
            var Start : {
                (input: string, k: {
                    (x: string): void;
                }): void;
            };
            var Main : {
                (): _Html1.Element;
            };
        }
        interface Action {
        }
        interface Website {
        }
    }
    
    import _List = IntelliFactory.WebSharper.List;
    import _Html = IntelliFactory.Html.Html;
    import _Web = IntelliFactory.WebSharper.Web;
    import _Html1 = IntelliFactory.WebSharper.Html;
}
