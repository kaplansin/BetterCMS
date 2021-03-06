﻿using System;
using System.Collections.Generic;
using System.Linq;

using BetterCms.Core.DataContracts.Enums;
using BetterCms.Module.Pages.Models;
using BetterCms.Module.Pages.ViewModels.Widgets;
using BetterCms.Module.Root.Models;
using BetterCms.Module.Root.Mvc;
using BetterCms.Module.Root.Services;

using CategoryEntity = BetterCms.Module.Root.Models.Category;

namespace BetterCms.Module.Pages.Command.Widget.SaveWidget
{
    public class SaveHtmlContentWidgetCommand : SaveWidgetCommandBase<EditHtmlContentWidgetViewModel>
    {
        public virtual IContentService ContentService { get; set; }

        public virtual IOptionService OptionService { get; set; }

        /// <summary>
        /// Executes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override SaveWidgetResponse Execute(EditHtmlContentWidgetViewModel request)
        {
            if (request.Options != null)
            {
                OptionService.ValidateOptionKeysUniqueness(request.Options);
            }

            UnitOfWork.BeginTransaction();
            
            var widgetContent = GetHtmlContentWidgetFromRequest(request);

            HtmlContentWidget widget = (HtmlContentWidget)ContentService.SaveContentWithStatusUpdate(widgetContent, request.DesirableStatus);
            
            Repository.Save(widget);

            UnitOfWork.Commit();

            // Notify.
            if (widget.Status != ContentStatus.Preview)
            {
                if (request.Id == default(Guid))
                {
                    Events.PageEvents.Instance.OnWidgetCreated(widget);
                }
                else
                {
                    Events.PageEvents.Instance.OnWidgetUpdated(widget);
                }
            }

            HtmlContentWidget modifiedWidget = widget;
            if (request.DesirableStatus == ContentStatus.Draft && widget.History != null)
            {
                var draft = widget.History.FirstOrDefault(h => h is HtmlContentWidget && !h.IsDeleted && h.Status == ContentStatus.Draft) as HtmlContentWidget;
                if (draft != null)
                {
                    modifiedWidget = draft;
                }
            }

            return new SaveWidgetResponse
                    {
                        Id = modifiedWidget.Id,
                        OriginalId = widget.Id,
                        WidgetName = modifiedWidget.Name,
                        CategoryName = modifiedWidget.Category != null ? modifiedWidget.Category.Name : null,
                        Version = modifiedWidget.Version,
                        OriginalVersion = widget.Version,
                        WidgetType = WidgetType.HtmlContent.ToString(),
                        IsPublished = widget.Status == ContentStatus.Published,
                        HasDraft = widget.Status == ContentStatus.Draft || widget.History != null && widget.History.Any(f => f.Status == ContentStatus.Draft),
                        DesirableStatus = request.DesirableStatus,
                        PreviewOnPageContentId = request.PreviewOnPageContentId
                    };
        }

        private HtmlContentWidget GetHtmlContentWidgetFromRequest(EditHtmlContentWidgetViewModel request)
        {
            HtmlContentWidget content = new HtmlContentWidget();
            content.Id = request.Id;

            if (request.CategoryId.HasValue && !request.CategoryId.Value.HasDefaultValue())
            {
                content.Category = Repository.AsProxy<CategoryEntity>(request.CategoryId.Value);
            }
            else
            {
                content.Category = null;
            }

            if (request.Options != null)
            {
                content.ContentOptions = new List<ContentOption>();

                // NOTE: Loading custom options before saving.
                // In other case, when loading custom options from option service, nHibernate updates version number (nHibernate bug)
                var customOptionsIdentifiers = request.Options
                    .Where(o => o.Type == OptionType.Custom)
                    .Select(o => o.CustomOption.Identifier)
                    .Distinct()
                    .ToArray();
                var customOptions = OptionService.GetCustomOptionsById(customOptionsIdentifiers);

                foreach (var requestContentOption in request.Options)
                {
                    var contentOption = new ContentOption
                    {
                        Content = content,
                        Key = requestContentOption.OptionKey,
                        DefaultValue = OptionService.ClearFixValueForSave(requestContentOption.OptionKey, requestContentOption.Type, requestContentOption.OptionDefaultValue),
                        Type = requestContentOption.Type,
                        CustomOption = requestContentOption.Type == OptionType.Custom
                          ? customOptions.First(o => o.Identifier == requestContentOption.CustomOption.Identifier)
                          : null
                    };

                    OptionService.ValidateOptionValue(contentOption);

                    content.ContentOptions.Add(contentOption);
                }
            }

            content.Name = request.Name;
            content.Html = request.PageContent ?? string.Empty;
            content.UseHtml = request.EnableCustomHtml;
            content.UseCustomCss = request.EnableCustomCSS;
            content.CustomCss = request.CustomCSS;            
            content.UseCustomJs = request.EnableCustomJS;
            content.CustomJs = request.CustomJS;           
            content.Version = request.Version;
            content.EditInSourceMode = request.EditInSourceMode;

            return content;
        }
    }
}