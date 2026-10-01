using BookyMark.Components;
using BookyMark.Data;
using BookyMark.Features.Bookmarks;
using BookyMark.Features.Collections;
using BookyMark.Features.LinkMetadata;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddLinkMetadataService();
builder.Services.AddBookmarkFeatures();
builder.Services.AddCollectionFeatures();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
