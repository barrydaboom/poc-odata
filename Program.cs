
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.UriParser;
using Microsoft.OData;
using System;
using Microsoft.OData.ModelBuilder;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using System.Xml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder
    .Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(opt =>
    {
        /*
        opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter bearer token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });
        opt.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[]{}
            }
        });
        */
    })
    .AddTransient<ODataUriResolver>()
    .AddTransient<UriPathParser>()
    .AddTransient<ODataSimplifiedOptions>()
    .AddTransient<ODataUriParserSettings>()
    .AddSingleton<EdmModel>(sc =>
    {
        var model = new EdmModel();
        var ec = model.AddEntityContainer("http://localhost:7186/api", "default");
        var et = model.AddEntityType("http://localhost:7186/api", "foo");
        et.AddKeys(et.AddStructuralProperty("x", EdmPrimitiveTypeKind.Int64));
        ec.AddEntitySet("foo", et);
        return model;
    })
    ;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app
    .MapGet("/api/{entity}", async (HttpContext ctx) =>
    {
        var p = odata.create_odata_uri_parser(new Uri($"{ctx.Request.Path.Value![5..]}{ctx.Request.QueryString}", UriKind.Relative), app.Services);
        var rt = new Dictionary<string, object>
        {
            { "@odata.context", $"{p.ServiceRoot}$metadata#{p.Path.EdmType().FullName()}" },
            { "value", new object[]
                {
                    "foo",
                    "bar"
                }
            }
        };
        return rt;
    })
    .WithDisplayName("foo")
    .WithName("foo");
app
    .MapGet("/api/$metadata", (HttpContext ctx) =>
     {
         ctx.Response.ContentType= "application/xml";
         using var xml = XmlTextWriter.Create(ctx.Response.Body);
         SchemaWriter.TryWriteSchema(app.Services.GetService<EdmModel>(), xml, out IEnumerable<EdmError> errors);
     })
    .WithDisplayName("bar")
    .WithName("bar");

app.Run();

class odata
{
    internal static ODataUri create_odata_uri_parser(Uri uri, IServiceProvider sp)
        => new ODataUriParser(sp.GetService<EdmModel>(), new Uri("http://localhost:7186/api"), uri, sp).ParseUri();
}