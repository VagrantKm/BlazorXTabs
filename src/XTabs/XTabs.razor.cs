﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BlazorXTabs.Configuration;

using Microsoft.AspNetCore.Components;

namespace BlazorXTabs
{
    public partial class XTabs
    {
        #region Private Fields

        /// <summary>
        /// All the tabs contained in this XTabs instance.
        /// </summary>
        private IList<XTab> _tabContent = new List<XTab>();

        /// <summary>
        /// All the tabs contained in this XTabs instance.
        /// </summary>
        public IEnumerable<XTab> TabContent => _tabContent.AsEnumerable();

        #endregion Private Fields

        #region Public Properties

        [Inject]
        private NavigationManager _navigationManager { get; set; }

        /// <summary>
        /// Sets the XTabs RenderMode.
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
        /// Sets the XTabs ChildContent.
        /// XTab should be inserted here.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Sets the template that handles the previous click handler.
        /// </summary>
        [Parameter] public RenderFragment<PreviousStepsContext> PreviousStepsContent { get; set; }

        /// <summary>
        /// Sets the template that handles the next click handler.
        /// </summary>
        [Parameter] public RenderFragment<NextStepsContext> NextStepsContent { get; set; }

        /// <summary>
        /// Sets the wrapping container css class
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

        #endregion Public Properties

        #region Private Properties

        private XTab Active { get; set; }

        #endregion Private Properties

        #region Public Methods

        /// <summary>
        /// Notifies XTabs that there have been changes.
        /// <para>If there are children that depend on each other's state, you should notify this parent component that the state has changed.</para>
        /// </summary>
        public Task NotifyStateHasChangedAsync() => InvokeAsync(() => StateHasChanged());

        /// <summary>
        /// Notifies XTabs that there have been changes.
        /// <para>If there are children that depend on each other's state, you should notify this parent component that the state has changed.</para>
        /// </summary>
        public void NotifyStateHasChanged() => StateHasChanged();

        #endregion Public Methods

        #region Internal Methods

        internal void AddPage(XTab tab)
        {
            ///TODO: Using Titles for now. Probably should use an ID.
            if (RenderMode == RenderMode.Full && _tabContent.FirstOrDefault(x => x.Title == tab.Title) is XTab existingTab)
                SetActive(existingTab);
            else
            {
                _tabContent.Add(tab);
                if (_tabContent.Count == 1 || NewTabSetActive)
                    SetActive(tab);
                if (OnTabAdded.HasDelegate)
                    OnTabAdded.InvokeAsync(tab);
            }
            StateHasChanged();
        }

        #endregion Internal Methods

        #region Private Methods

        private void SetActive(XTab tab)
        {
            Active = tab;
            if (OnActiveTabChanged.HasDelegate)
                OnActiveTabChanged.InvokeAsync(tab);
        }

        private bool IsActive(XTab tab) => tab == Active;

        private void CloseTab(XTab tab)
        {
            var nextSelected = Active;
            if (Active == tab && _tabContent.Count > 1)
                for (int i = 0; i < _tabContent.Count; i++)
                {
                    if (i > 0 && _tabContent[i] == Active)
                        nextSelected = _tabContent[i - 1];
                    if (i > 0 && _tabContent[i - 1] == Active)
                        nextSelected = _tabContent[i];
                }

            _tabContent.Remove(tab);
            if (OnTabRemoved.HasDelegate)
                OnTabRemoved.InvokeAsync();

            SetActive(nextSelected);

            if (_tabContent.Count == 0)
                _navigationManager.NavigateTo("");

            StateHasChanged();
        }

        #endregion Private Methods

        #region Steps

        private bool IsTabHeaderDisabled => RenderMode == RenderMode.Steps;
        private bool IsPreviousDisabled => (_tabContent?.Count > 0 && _tabContent.IndexOf(Active) == 0);

        private bool IsNextDisabled => (_tabContent?.Count > 0 && _tabContent.IndexOf(Active) == _tabContent.IndexOf(_tabContent.Last()));

        private void NextTab()
        {
            var next = _tabContent.IndexOf(Active) + 1;
            SetActive(_tabContent[next]);
            if (OnNextSteps.HasDelegate)
                OnNextSteps.InvokeAsync();
        }

        private void PreviousTab()
        {
            var previous = _tabContent.IndexOf(Active) - 1;
            SetActive(_tabContent[previous]);
            if (OnPreviousSteps.HasDelegate)
                OnPreviousSteps.InvokeAsync();
        }

        #endregion Steps
    }
}