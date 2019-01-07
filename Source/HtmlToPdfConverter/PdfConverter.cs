﻿using DinkToPdf;
using DinkToPdf.Contracts;
using HtmlToPdfConverter.Contracts;
using HtmlToPdfConverter.Contracts.PageSettings;
using HtmlToPdfConverter.Validator;
using System;
using System.Threading.Tasks;

namespace HtmlToPdfConverter
{
    public class PdfConverter : IPdfConverter
    {
        private readonly IConverter _converter;

        public PdfConverter(IConverter converter)
        {
            _converter = converter;
        }

        public async Task<byte[]> CreatePdfDocument(PdfBuildModel buildModel, IBasePdfPageSpecification pdfPageSpecification)
        {
            try
            {
                if (!buildModel.IsValid()) throw new ArgumentNullException(nameof(buildModel));

                GlobalSettings globalSettings = await GetGlobalSettings(buildModel, pdfPageSpecification);
                ObjectSettings objectSettings = await GetDefaultObjectSettings(buildModel, pdfPageSpecification);

                HtmlToPdfDocument pdfDocument = await ConvertToPdf(globalSettings, objectSettings);

                return _converter.Convert(pdfDocument);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(nameof(CreatePdfDocument), ex);
            }
        }

        private static FooterSettings BuildDefaultFooterSettings(PdfBuildModel buildModel, IBasePdfPageSpecification pdfPageSpecification)
        {
            return new FooterSettings
            {
                FontName = pdfPageSpecification.FontName,
                FontSize = pdfPageSpecification.FontSize,
                Line = buildModel.UseFooterLine,
                Left = buildModel.FooterLeftText ?? string.Empty,
                Center = buildModel.FooterCenterText ?? string.Empty,
                Right = buildModel.FooterRightText ?? string.Empty,
                Spacing = pdfPageSpecification.PageSpacing,
                HtmUrl = buildModel.HtmlUri ?? string.Empty
            };
        }

        private static HeaderSettings BuildDefaultHeaderSettings(PdfBuildModel buildModel, IBasePdfPageSpecification pdfPageSpecification)
        {
            return new HeaderSettings
            {
                FontName = pdfPageSpecification.FontName,
                FontSize = pdfPageSpecification.FontSize,
                Line = buildModel.UseHeaderLine,
                Left = buildModel.HeaderLeftText ?? string.Empty,
                Center = buildModel.HeaderCenterText ?? string.Empty,
                Right = buildModel.HeaderRightText ?? string.Empty,
                Spacing = pdfPageSpecification.PageSpacing
            };
        }

        private static WebSettings BuildDefaultWebSettings(PdfBuildModel buildModel, IBasePdfPageSpecification pdfPageSpecification)
        {
            return new WebSettings
            {
                DefaultEncoding = pdfPageSpecification.DefaultEncoding,
                UserStyleSheet = pdfPageSpecification.UserStyleSheet,
            };
        }

        private Task<HtmlToPdfDocument> ConvertToPdf(GlobalSettings globalSettings, ObjectSettings objectSettings)
        {
            if (globalSettings == null || objectSettings == null)
            {
                throw new ArgumentNullException(nameof(ConvertToPdf));
            }

            var pdf = new HtmlToPdfDocument
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            return Task.FromResult(pdf);
        }

        private Task<ObjectSettings> GetDefaultObjectSettings(PdfBuildModel buildModel, IBasePdfPageSpecification pdfPageSpecification)
        {
            if (string.IsNullOrWhiteSpace(buildModel.HtmlContent))
            {
                throw new ArgumentNullException(nameof(GetDefaultObjectSettings));
            }

            var result = new ObjectSettings
            {
                FooterSettings = BuildDefaultFooterSettings(buildModel, pdfPageSpecification),
                HeaderSettings = BuildDefaultHeaderSettings(buildModel, pdfPageSpecification),
                HtmlContent = buildModel.HtmlContent,
                PagesCount = buildModel.UsePageCount,
                WebSettings = BuildDefaultWebSettings(buildModel, pdfPageSpecification),
            };

            return Task.FromResult(result);
        }

        private Task<GlobalSettings> GetGlobalSettings(PdfBuildModel buildModel, IBasePdfPageSpecification pdfPageSpecification)
        {
            if (!buildModel.IsValid()) throw new ArgumentNullException(nameof(BuildDefaultWebSettings));

            var result = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 20, Bottom = 20 },
                DocumentTitle = buildModel.DocumentTitle,
            };

            return Task.FromResult(result);
        }
    }
}