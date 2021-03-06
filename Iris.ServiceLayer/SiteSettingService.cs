﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EFSecondLevelCache;
using Iris.DataLayer;
using Iris.DomainClasses;
using Iris.ServiceLayer.Contracts;
using Iris.ViewModels;

namespace Iris.ServiceLayer
{
    public enum SettingsName
    {
        SiteName,
        ContactInfo,
        SiteKeywords,
        SiteDescription
    }

    public class SiteSettingService : ISiteSettingService
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IMappingEngine _mappingEngine;
        private readonly IDbSet<SiteOption> _SiteOptions;

        public SiteSettingService(IUnitOfWork unitOfWork, IMappingEngine mappingEngine)
        {
            _UnitOfWork = unitOfWork;
            _mappingEngine = mappingEngine;
            _SiteOptions = unitOfWork.Set<SiteOption>();
        }

        public async Task UpdateSettings(EditSettingViewModel settingsModel)
        {
            var currentSettings = await _SiteOptions.ToListAsync();

            currentSettings.FirstOrDefault(s => s.Key == "SiteUrl").Value = settingsModel.SiteUrl;
            currentSettings.FirstOrDefault(s => s.Key == "SiteName").Value = settingsModel.SiteName;
            currentSettings.FirstOrDefault(s => s.Key == "SiteKeywords").Value = settingsModel.SiteKeywords;
            currentSettings.FirstOrDefault(s => s.Key == "SiteDescription").Value = settingsModel.SiteDescription;
            currentSettings.FirstOrDefault(s => s.Key == "MailServerUrl").Value = settingsModel.MailServerUrl;
            currentSettings.FirstOrDefault(s => s.Key == "MailServerPort").Value = settingsModel.MailServerPort;
            currentSettings.FirstOrDefault(s => s.Key == "MailServerUserName").Value = settingsModel.MailServerUserName;
            currentSettings.FirstOrDefault(s => s.Key == "MailServerPassword").Value = settingsModel.MailServerPassword;

            if (!string.IsNullOrWhiteSpace(settingsModel.ContactInfo))
            {
                currentSettings.FirstOrDefault(s => s.Key == "ContactInfo").Value = settingsModel.ContactInfo.Replace(Environment.NewLine, "<br/>");
            }

        }

        public async Task<EditSettingViewModel> GetSettingsForEdit()
        {
            var settings = await _SiteOptions.AsNoTracking().ToListAsync();

            var settingsViewModel = new EditSettingViewModel
            {
                SiteUrl = settings.FirstOrDefault(option => option.Key == "SiteUrl")?.Value,
                SiteName = settings.FirstOrDefault(option => option.Key == "SiteName")?.Value,
                SiteKeywords = settings.FirstOrDefault(option => option.Key == "SiteKeywords")?.Value,
                SiteDescription = settings.FirstOrDefault(option => option.Key == "SiteDescription")?.Value,
                MailServerUrl = settings.FirstOrDefault(option => option.Key == "MailServerUrl")?.Value,
                MailServerPort = settings.FirstOrDefault(option => option.Key == "MailServerPort")?.Value,
                MailServerUserName = settings.FirstOrDefault(option => option.Key == "MailServerUserName")?.Value,
                MailServerPassword = settings.FirstOrDefault(option => option.Key == "MailServerPassword")?.Value,
                ContactInfo = settings.FirstOrDefault(option => option.Key == "ContactInfo")?.Value,
            };

            return settingsViewModel;
        }

        public async Task<string> GetSiteName()
        {
            return (await GetSettingsForEdit()).SiteName;
        }

        public async Task<string> GetContactInfo()
        {
            return (await GetSettingsForEdit()).ContactInfo;
        }

        public async Task<SiteMetaTags> GetSiteMetaTags()
        {
            return new SiteMetaTags
            {
                Keywords = (await GetSettingsForEdit()).ContactInfo,
                Description = (await GetSettingsForEdit()).ContactInfo
            };
        }

        public async Task<string> GetNotificationEmail()
        {
            return await _SiteOptions
                .Where(option => option.Key == "NotificationEmail")
                .Select(option => option.Value)
                .FirstOrDefaultAsync();
        }

        public async Task<SmtpSettings> GetSmtpSettings()
        {
            var settings = await GetSettingsForEdit();
            var smtpSettings = new SmtpSettings
            {
                Host = settings.MailServerUrl,
                Password = settings.MailServerPassword,
                UserName = settings.MailServerUserName
            };

            if (!string.IsNullOrWhiteSpace(settings.MailServerPort))
            {
                smtpSettings.Port = Convert.ToInt32(settings.MailServerPort);
            }

            return smtpSettings;
        }

        public EditSettingViewModel GetSettings()
        {
            var settings = _SiteOptions.AsNoTracking().ToList();

            var settingsViewModel = new EditSettingViewModel
            {
                SiteUrl = settings.FirstOrDefault(option => option.Key == "SiteUrl")?.Value,
                SiteName = settings.FirstOrDefault(option => option.Key == "SiteName")?.Value,
                SiteKeywords = settings.FirstOrDefault(option => option.Key == "SiteKeywords")?.Value,
                SiteDescription = settings.FirstOrDefault(option => option.Key == "SiteDescription")?.Value,
                MailServerUrl = settings.FirstOrDefault(option => option.Key == "MailServerUrl")?.Value,
                MailServerPort = settings.FirstOrDefault(option => option.Key == "MailServerPort")?.Value,
                MailServerUserName = settings.FirstOrDefault(option => option.Key == "MailServerUserName")?.Value,
                MailServerPassword = settings.FirstOrDefault(option => option.Key == "MailServerPassword")?.Value,
                ContactInfo = settings.FirstOrDefault(option => option.Key == "ContactInfo")?.Value,
            };

            return settingsViewModel;
        }

        private string GetValue(string key)
        {
            return _SiteOptions
                   .Where(option => option.Key == key)
                        .Select(option => option.Value)
                           .Cacheable()
                               .FirstOrDefault();
        }

        private Task<string> GetValueAsync(string key)
        {
            return _SiteOptions
                   .Where(option => option.Key == key)
                        .Select(option => option.Value)
                           .Cacheable()
                               .FirstOrDefaultAsync();
        }

    }
}
