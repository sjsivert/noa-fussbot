#pragma checksum "/Users/andreas.schafferer/Projects/makingfuss/Views/Contester/Create.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "c8097f490e822da8c0cd9e4cb9dae8a41d04766a"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Contester_Create), @"mvc.1.0.view", @"/Views/Contester/Create.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "/Users/andreas.schafferer/Projects/makingfuss/Views/_ViewImports.cshtml"
using vscodecore;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "/Users/andreas.schafferer/Projects/makingfuss/Views/_ViewImports.cshtml"
using vscodecore.Models;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"c8097f490e822da8c0cd9e4cb9dae8a41d04766a", @"/Views/Contester/Create.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"e91d238906673d7228a284d7701ae2d6c98f815e", @"/Views/_ViewImports.cshtml")]
    public class Views_Contester_Create : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<Contester>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 2 "/Users/andreas.schafferer/Projects/makingfuss/Views/Contester/Create.cshtml"
  
    ViewBag.Title = "New Contester";

#line default
#line hidden
#nullable disable
            WriteLiteral("<h1>");
#nullable restore
#line 5 "/Users/andreas.schafferer/Projects/makingfuss/Views/Contester/Create.cshtml"
Write(ViewBag.Title);

#line default
#line hidden
#nullable disable
            WriteLiteral("</h1>\n");
#nullable restore
#line 6 "/Users/andreas.schafferer/Projects/makingfuss/Views/Contester/Create.cshtml"
 using(Html.BeginForm()){

#line default
#line hidden
#nullable disable
            WriteLiteral("  <div class=\"form-group\">\n    ");
#nullable restore
#line 8 "/Users/andreas.schafferer/Projects/makingfuss/Views/Contester/Create.cshtml"
Write(Html.LabelFor(model => model.FirstName));

#line default
#line hidden
#nullable disable
            WriteLiteral("\n    ");
#nullable restore
#line 9 "/Users/andreas.schafferer/Projects/makingfuss/Views/Contester/Create.cshtml"
Write(Html.TextBoxFor(model => model.FirstName, new { @class="form-control"}));

#line default
#line hidden
#nullable disable
            WriteLiteral(" \n  </div>\n  <div class=\"form-group\">\n    ");
#nullable restore
#line 12 "/Users/andreas.schafferer/Projects/makingfuss/Views/Contester/Create.cshtml"
Write(Html.LabelFor(model => model.LastName));

#line default
#line hidden
#nullable disable
            WriteLiteral("\n    ");
#nullable restore
#line 13 "/Users/andreas.schafferer/Projects/makingfuss/Views/Contester/Create.cshtml"
Write(Html.TextBoxFor(model => model.LastName, new { @class="form-control"}));

#line default
#line hidden
#nullable disable
            WriteLiteral(" \n  </div>\n  <button type=\"submit\" class=\"btn btn-warning\">Submit</button>\n");
#nullable restore
#line 16 "/Users/andreas.schafferer/Projects/makingfuss/Views/Contester/Create.cshtml"
}

#line default
#line hidden
#nullable disable
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<Contester> Html { get; private set; }
    }
}
#pragma warning restore 1591
