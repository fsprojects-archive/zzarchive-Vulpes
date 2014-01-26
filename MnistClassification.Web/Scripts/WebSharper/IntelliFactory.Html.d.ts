declare module IntelliFactory {
    module Html {
        module Web {
            interface Control {
            }
        }
        module Html {
            interface TagContent<_T1> {
                Name: string;
                Attributes: _List.T<any>;
                Contents: _List.T<_Html.Element<_T1>>;
                Annotation: _WebSharper.OptionProxy<_T1>;
            }
            interface Element<_T1> {
            }
            interface Attribute<_T1> {
                Name: string;
                Value: string;
            }
            interface INode<_T1> {
                get_Node(): _Html.Node<_T1>;
            }
            interface Node<_T1> {
            }
            interface IElement<_T1> {
                get_Element(): _Html.Element<_T1>;
            }
            interface Document<_T1> {
                Annotations: any;
                Body: _Html.Element<void>;
            }
            interface Writer {
            }
        }
    }
    
    import _List = IntelliFactory.WebSharper.List;
    import _Html = IntelliFactory.Html.Html;
    import _WebSharper = IntelliFactory.WebSharper;
}
