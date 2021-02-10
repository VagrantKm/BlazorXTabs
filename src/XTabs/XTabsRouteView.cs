﻿using System;
using System.Reflection;
using System.Threading.Tasks;

using BlazorXTabs.Configuration;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorXTabs
{
    public class XTabsRouteView : IComponent
    {
        #region Private Fields

        private readonly RenderFragment _renderDelegate;
        private readonly RenderFragment _renderPageWithParametersDelegate;
        private RenderHandle _renderHandle;

        private XTabs _xTabs;
        private RenderFragment _xTabsRenderFragment;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RouteView"/>.
        /// </summary>
        public XTabsRouteView()
        {
            // Cache the delegate instances
            _renderDelegate = Render;
            _renderPageWithParametersDelegate = RenderPageWithParameters;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the route data. This determines the page that will be
        /// displayed and the parameter values that will be supplied to the page.
        /// </summary>
        [Parameter]
        public RouteData RouteData { get; set; }

        /// <summary>
        /// Gets or sets the type of a layout to be used if the page does not
        /// declare any layout. If specified, the type must implement <see cref="IComponent"/>
        /// and accept a parameter named <see cref="LayoutComponentBase.Body"/>.
        /// </summary>
        [Parameter]
        public Type DefaultLayout { get; set; }

        /// <summary>
        /// Gets or sets the XTabs RenderMode.
        /// <code>Defaults to: <see cref="RenderMode.Partial" /></code>
        /// </summary>
        [Parameter] public RenderMode RenderMode { get; set; } = RenderMode.Partial;

        /// <summary>
        /// Can close tabs.
        /// </summary>
        [Parameter] public bool CloseTabs { get; set; }

        /// <summary>
        /// When a new tab is added, sets it to active.
        /// </summary>
        [Parameter] public bool NewTabSetActive { get; set; }

        /// <summary>
        /// Gets or sets the XTabs ChildContent.
        /// XTab should be inserted here.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Gets or sets the template that handles the previous click handler.
        /// </summary>
        [Parameter] public RenderFragment<PreviousStepsContext> PreviousStepsContent { get; set; }

        /// <summary>
        /// Gets or sets the template that handles the next click handler.
        /// </summary>
        [Parameter] public RenderFragment<NextStepsContext> NextStepsContent { get; set; }

        /// <summary>
        /// Gets or sets the wrapping container css class
        /// </summary>
        [Parameter] public string CssClass { get; set; }

        /// <summary>
        /// Event: When a tab is added to XTabs.
        /// </summary>
        [Parameter] public EventCallback<XTab> OnTabAdded { get; set; }

        /// <summary>
        /// Event: When the active tab is changed on XTabs.
        /// </summary>
        [Parameter] public EventCallback<XTab> OnActiveTabChanged { get; set; }

        /// <summary>
        /// Event: When a tab is removed from XTabs.
        /// </summary>
        [Parameter] public EventCallback<XTab> OnTabRemoved { get; set; }

        /// <summary>
        /// Event: When on XTabs Steps Mode, triggers on previous click.
        /// </summary>
        [Parameter] public EventCallback OnPreviousSteps { get; set; }

        /// <summary>
        /// Event: When on XTabs Steps Mode, triggers on next click.
        /// </summary>
        [Parameter] public EventCallback OnNextSteps { get; set; }

        /// <summary>
        /// Sets the active tab's loading state.
        /// </summary>
        [Parameter]
        public bool IsLoading { get; set; }

        /// <summary>
        /// Gets or sets the XTabs's drag feature.
        /// </summary>
        [Parameter]
        public bool IsDraggable { get; set; }

        #endregion

        #region Private Methods

        private RenderFragment RenderNewPage(out string xTabTitle)
        {
            //Let's make sure not to capture the RouteData reference in the delegate.
            var pageType = RouteData.PageType;
            var values = RouteData.RouteValues;
            xTabTitle = pageType.Name;
            var pageAttr = pageType.GetCustomAttribute<XTabPageAttribute>();
            if (pageAttr is not null)
                xTabTitle = pageAttr.Title;

            return new RenderFragment(rBuilder =>
            {
                rBuilder.OpenComponent(0, pageType);
                foreach (var kvp in values)
                    rBuilder.AddAttribute(1, kvp.Key, kvp.Value);
                rBuilder.CloseComponent();
            });
        }

        private void RenderPageWithParameters(RenderTreeBuilder builder)
        {
            var pageFragment = RenderNewPage(out var xTabTitle);

            if (_xTabs is null)
            {
                var xTabFragment = new RenderFragment(rBuilder =>
                {
                    rBuilder.OpenComponent(0, typeof(XTab));
                    rBuilder.AddAttribute(1, nameof(XTab.ChildContent), pageFragment);
                    rBuilder.AddAttribute(2, nameof(XTab.Title), xTabTitle);
                    rBuilder.CloseComponent();
                });

                _xTabsRenderFragment = new RenderFragment(rBuilder =>
                {
                    rBuilder.OpenComponent<XTabs>(0);

                    rBuilder.AddAttribute(1, nameof(XTabs.RenderMode), RenderMode);
                    rBuilder.AddAttribute(2, nameof(XTabs.CloseTabs), CloseTabs);
                    rBuilder.AddAttribute(3, nameof(XTabs.NewTabSetActive), NewTabSetActive);
                    rBuilder.AddAttribute(4, nameof(XTabs.ChildContent), xTabFragment);
                    rBuilder.AddAttribute(5, nameof(XTabs.PreviousStepsContent), PreviousStepsContent);
                    rBuilder.AddAttribute(6, nameof(XTabs.NextStepsContent), NextStepsContent);
                    rBuilder.AddAttribute(7, nameof(XTabs.CssClass), CssClass);
                    rBuilder.AddAttribute(8, nameof(XTabs.OnTabAdded), OnTabAdded);
                    rBuilder.AddAttribute(9, nameof(XTabs.OnActiveTabChanged), OnActiveTabChanged);
                    rBuilder.AddAttribute(10, nameof(XTabs.OnTabRemoved), OnTabRemoved);
                    rBuilder.AddAttribute(11, nameof(XTabs.OnPreviousSteps), OnPreviousSteps);
                    rBuilder.AddAttribute(12, nameof(XTabs.OnNextSteps), OnNextSteps);
                    rBuilder.AddAttribute(13, nameof(XTabs.IsLoading), IsLoading);
                    rBuilder.AddAttribute(14, nameof(XTabs.IsDraggable), IsDraggable);

                    rBuilder.AddComponentReferenceCapture(13, compRef => _xTabs = (XTabs)compRef);
                    rBuilder.CloseComponent();
                });
            }
            else
            {
                var xtab = new XTab(_xTabs, xTabTitle, pageFragment);
                _xTabs.AddPage(xtab);
            }

            builder.OpenElement(0, "XTabs");
            builder.AddContent(1, _xTabsRenderFragment);
            builder.CloseElement();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <param name="builder">The <see cref="RenderTreeBuilder"/>.</param>
        protected virtual void Render(RenderTreeBuilder builder)
        {
            var pageLayoutType = RouteData.PageType.GetCustomAttribute<LayoutAttribute>()?.LayoutType
                ?? DefaultLayout;

            builder.OpenComponent<LayoutView>(0);
            builder.AddAttribute(1, nameof(LayoutView.Layout), pageLayoutType);
            builder.AddAttribute(2, nameof(LayoutView.ChildContent), _renderPageWithParametersDelegate);
            builder.CloseComponent();
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public void Attach(RenderHandle renderHandle)
            => _renderHandle = renderHandle;

        /// <inheritdoc />
        public Task SetParametersAsync(ParameterView parameters)
        {
            parameters.SetParameterProperties(this);

            if (RouteData == null)
            {
                throw new InvalidOperationException($"The {nameof(RouteView)} component requires a non-null value for the parameter {nameof(RouteData)}.");
            }

            _renderHandle.Render(_renderDelegate);
            return Task.CompletedTask;
        }

        #endregion
    }
}