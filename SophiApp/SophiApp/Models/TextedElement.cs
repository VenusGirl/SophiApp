﻿using SophiApp.Commons;
using SophiApp.Dto;
using SophiApp.Helpers;
using SophiApp.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace SophiApp.Models
{
    internal class TextedElement : IElement
    {
        private string description;
        private string header;
        private ElementStatus status;

        public TextedElement((TextedElementDto Dto, Action<TextedElement, Exception> ErrorHandler,
                                EventHandler<TextedElement> StatusHandler, Func<bool> Customisation, UILanguage Language) parameters)
        {
            Headers = parameters.Dto.Header ?? parameters.Dto.ChildHeader;
            Descriptions = parameters.Dto.Description ?? parameters.Dto.ChildDescription;
            Id = parameters.Dto.Id;
            Tag = parameters.Dto.Tag;
            ErrorOccurred = parameters.ErrorHandler;
            StatusChanged = parameters.StatusHandler;
            Language = parameters.Language;
            CustomisationStatus = parameters.Customisation;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<TextedElement> StatusChanged;

        protected Dictionary<UILanguage, string> Descriptions { get; set; }

        internal Func<bool> CustomisationStatus { get; set; }

        internal Action<TextedElement, Exception> ErrorOccurred { get; set; }

        internal UILanguage Language { get; set; }

        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }

        public string Header
        {
            get => header;
            set
            {
                header = value;
                OnPropertyChanged("Header");
            }
        }

        public Dictionary<UILanguage, string> Headers { get; set; }
        public uint Id { get; }

        public ElementStatus Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged("Status");
                StatusChanged?.Invoke(null, this);
            }
        }

        public string Tag { get; }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        internal void ChangeStatus() => Status = Status == ElementStatus.UNCHECKED ? ElementStatus.CHECKED : ElementStatus.UNCHECKED;

        internal virtual bool ContainsText(string text)
        {
            var desiredText = text.ToLower();
            return Header.ToLower().Contains(desiredText) || Description.ToLower().Contains(desiredText);
        }

        internal virtual void GetCustomisationStatus()
        {
            try
            {
                Status = CustomisationStatus.Invoke() ? ElementStatus.CHECKED : ElementStatus.UNCHECKED;
            }
            catch (Exception e)
            {
                ErrorOccurred?.Invoke(this, e);
            }
        }

        internal virtual void Initialize()
        {
            var stopwatch = Stopwatch.StartNew();
            ChangeLanguage(Language);
            GetCustomisationStatus();
            stopwatch.Stop();
            DebugHelper.TextedElementInit(Id, stopwatch.Elapsed.TotalSeconds);
        }

        public virtual void ChangeLanguage(UILanguage language)
        {
            Header = Headers[language];
            Description = Descriptions[language];
        }
    }
}