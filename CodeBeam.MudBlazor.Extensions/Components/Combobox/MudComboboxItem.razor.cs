﻿using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Utilities;
using MudExtensions.Enums;

namespace MudExtensions
{
    public partial class MudComboboxItem<T> : MudBaseSelectItem, IDisposable
    {
        protected string Classname => new CssBuilder("mud-combobox-item")
            .AddClass($"mud-combobox-item-{MudCombobox?.Dense.ToDescriptionString()}")
            .AddClass("mud-ripple", !DisableRipple && !Disabled)
            .AddClass("mud-combobox-item-gutters")
            .AddClass("mud-combobox-item-clickable")
            .AddClass("mud-combobox-item-hilight", Active && !Disabled)
            .AddClass("mud-combobox-item-hilight-selected", Active && Selected && !Disabled)
            .AddClass($"mud-selected-item mud-{MudCombobox?.Color.ToDescriptionString()}-text mud-{MudCombobox?.Color.ToDescriptionString()}-hover", Selected && !Disabled && !Active)
            .AddClass("mud-combobox-item-disabled", Disabled)
            .AddClass("mud-combobox-item-bordered", MudCombobox?.Bordered == true && Active)
            .AddClass($"mud-combobox-item-bordered-{MudCombobox?.Color.ToDescriptionString()}", MudCombobox?.Bordered == true && Selected)
            .AddClass("d-none", Eligible == false)
            .AddClass(Class)
            .Build();

        internal string ItemId { get; } = "_" + Guid.NewGuid().ToString().Substring(0, 8);

        /// <summary>
        /// The parent select component
        /// </summary>
        [CascadingParameter]
        internal MudCombobox<T> MudCombobox { get; set; }

        protected Typo GetTypo()
        {
            if (MudCombobox == null)
            {
                return Typo.body1;
            }

            if (MudCombobox.Dense == Dense.Slim || MudCombobox.Dense == Dense.Superslim)
            {
                return Typo.body2;
            }

            return Typo.body1;
        }

        /// <summary>
        /// Functional items does not hold values. If a value set on Functional item, it ignores by the MudSelect. They cannot be subject of keyboard navigation and selection.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.List.Behavior)]
        public bool IsFunctional { get; set; }

        /// <summary>
        /// The text to display
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.List.Behavior)]
        public string Text { get; set; }

        /// <summary>
        /// A user-defined option that can be selected
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public T Value { get; set; }

        protected internal bool Selected { get; set; }
        protected internal bool Active { get; set; }

        public void SetActive(bool isActive)
        {
            Active = isActive;
            StateHasChanged();
        }

        [Parameter]
        public bool Eligible { get; set; } = true;

        protected string DisplayString
        {
            get
            {
                var converter = MudCombobox?.Converter;
                if (MudCombobox?.ItemPresenter == ValuePresenter.None)
                {
                    if (converter == null)
                        return Value.ToString();
                    return converter.Set(Value);
                }
                
                if (converter == null)
                    return $"{(string.IsNullOrEmpty(Text) ? Value : Text)}";
                return !string.IsNullOrEmpty(Text) ? Text : converter.Set(Value);
            }
        }

        public void ForceRender()
        {
            CheckEligible();
            StateHasChanged();
        }

        public async Task ForceUpdate()
        {
            SyncSelected();
            await InvokeAsync(StateHasChanged);
        }

        protected override void OnInitialized()
        {
            MudCombobox?.Add(this);
        }

        bool? _oldMultiselection;
        bool? _oldSelected;
        bool _selectedChanged = false;
        bool? _oldEligible = true;
        bool _eligibleChanged = false;
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            //SyncSelected();
            //if (_oldSelected != Selected || _oldEligible != Eligible)
            //{
            //    _selectedChanged = true;
            //}
            //_oldSelected = Selected;
            CheckEligible();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (_selectedChanged)
            {
                _selectedChanged = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected internal void CheckEligible()
        {
            Eligible = IsEligible();
        }

        protected bool IsEligible()
        {
            if (MudCombobox == null || MudCombobox.Editable == false)
            {
                return true;
            }

            if (string.IsNullOrEmpty(MudCombobox._searchString))
            {
                return true;
            }

            if (MudCombobox?.SearchFunc != null)
            {
                return MudCombobox.SearchFunc.Invoke(Value, Text, MudCombobox.GetSearchString());
            }

            if (string.IsNullOrEmpty(Text) == false)
            {
                if (Text.Contains(MudCombobox._searchString ?? string.Empty, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            else
            {
                if (MudCombobox.Converter.Set(Value).Contains(MudCombobox._searchString ?? string.Empty, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        protected void SyncSelected()
        {
            if (MudCombobox == null)
            {
                return;
            }

            if (MudCombobox.MultiSelection == true && MudCombobox.SelectedValues.Contains(Value)) 
            {
                Selected = true;
            }
            else if (MudCombobox.MultiSelection == false && ((MudCombobox.Value == null && Value == null) || MudCombobox.Value?.Equals(Value) == true))
            {
                Selected = true;
            }
            else
            {
                Selected = false;
            }
        }

        protected async Task HandleOnClick()
        {
            //Selected = !Selected;
            await MudCombobox.ToggleOption(this, !Selected);
            //await MudCombobox?.SelectOption(Value);
            await InvokeAsync(StateHasChanged);
            //if (MudCombobox.MultiSelection == false)
            //{
            //    await MudCombobox?.CloseMenu();
            //}
            //else
            //{
            //    await MudCombobox.FocusAsync();
            //}
            await MudCombobox.FocusAsync();
            await OnClick.InvokeAsync();
        }

        protected bool GetDisabledStatus()
        {
            if (MudCombobox?.ItemDisabledFunc != null)
            {
                return MudCombobox.ItemDisabledFunc(Value);
            }
            return Disabled;
        }

        public void Dispose()
        {
            try
            {
                MudCombobox?.Remove(this);
            }
            catch (Exception) { }
        }
    }
}