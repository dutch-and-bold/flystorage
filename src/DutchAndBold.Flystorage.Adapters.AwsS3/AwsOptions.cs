using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Amazon.Runtime;
using Amazon.S3;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Adapters.AwsS3
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class AwsOptions : Dictionary<string, object>
    {
        public S3CannedACL CannedACL
        {
            get => this.GetValueOrDefault(nameof(CannedACL))?.ToString();
            set => this[nameof(CannedACL)] = value;
        }

        public object CacheControl
        {
            get => this.GetValueOrDefault(nameof(CacheControl));
            set => this[nameof(CacheControl)] = value;
        }

        public object ContentDisposition
        {
            get => this.GetValueOrDefault(nameof(ContentDisposition));
            set => this[nameof(ContentDisposition)] = value;
        }

        public object ContentEncoding
        {
            get => this.GetValueOrDefault(nameof(ContentEncoding));
            set => this[nameof(ContentEncoding)] = value;
        }

        public object ContentLength
        {
            get => this.GetValueOrDefault(nameof(ContentLength));
            set => this[nameof(ContentLength)] = value;
        }

        public object ContentType
        {
            get => this.GetValueOrDefault(nameof(ContentType));
            set => this[nameof(ContentType)] = value;
        }

        public object Expires
        {
            get => this.GetValueOrDefault(nameof(Expires));
            set => this[nameof(Expires)] = value;
        }

        public object GrantFullControl
        {
            get => this.GetValueOrDefault(nameof(GrantFullControl));
            set => this[nameof(GrantFullControl)] = value;
        }

        public object GrantRead
        {
            get => this.GetValueOrDefault(nameof(GrantRead));
            set => this[nameof(GrantRead)] = value;
        }

        public object GrantReadACP
        {
            get => this.GetValueOrDefault(nameof(GrantReadACP));
            set => this[nameof(GrantReadACP)] = value;
        }

        public object GrantWriteACP
        {
            get => this.GetValueOrDefault(nameof(GrantWriteACP));
            set => this[nameof(GrantWriteACP)] = value;
        }

        public object Metadata
        {
            get => this.GetValueOrDefault(nameof(Metadata));
            set => this[nameof(Metadata)] = value;
        }

        public object MetadataDirective
        {
            get => this.GetValueOrDefault(nameof(MetadataDirective));
            set => this[nameof(MetadataDirective)] = value;
        }

        public object RequestPayer
        {
            get => this.GetValueOrDefault(nameof(RequestPayer));
            set => this[nameof(RequestPayer)] = value;
        }

        public object SSECustomerAlgorithm
        {
            get => this.GetValueOrDefault(nameof(SSECustomerAlgorithm));
            set => this[nameof(SSECustomerAlgorithm)] = value;
        }

        public object SSECustomerKey
        {
            get => this.GetValueOrDefault(nameof(SSECustomerKey));
            set => this[nameof(SSECustomerKey)] = value;
        }

        public object SSECustomerKeyMD5
        {
            get => this.GetValueOrDefault(nameof(SSECustomerKeyMD5));
            set => this[nameof(SSECustomerKeyMD5)] = value;
        }

        public object SSEKMSKeyId
        {
            get => this.GetValueOrDefault(nameof(SSEKMSKeyId));
            set => this[nameof(SSEKMSKeyId)] = value;
        }

        public object ServerSideEncryption
        {
            get => this.GetValueOrDefault(nameof(ServerSideEncryption));
            set => this[nameof(ServerSideEncryption)] = value;
        }

        public object StorageClass
        {
            get => this.GetValueOrDefault(nameof(StorageClass));
            set => this[nameof(StorageClass)] = value;
        }

        public object Tagging
        {
            get => this.GetValueOrDefault(nameof(Tagging));
            set => this[nameof(Tagging)] = value;
        }

        public object WebsiteRedirectLocation
        {
            get => this.GetValueOrDefault(nameof(WebsiteRedirectLocation));
            set => this[nameof(WebsiteRedirectLocation)] = value;
        }

        public static AwsOptions Create(Config config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var awsOptions = new AwsOptions();

            foreach (var fieldInfo in typeof(AwsOptions).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var value = config.Get<object>(fieldInfo.Name);
                if (value == null)
                {
                    continue;
                }

                fieldInfo.SetValue(awsOptions, value);
            }

            return awsOptions;
        }

        public T CreateNew<T>()
            where T : AmazonWebServiceRequest, new()
        {
            var awsModel = new T();

            foreach (var fieldInfo in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!ContainsKey(fieldInfo.Name))
                {
                    continue;
                }

                var value = this[fieldInfo.Name];

                if (fieldInfo.PropertyType.IsEnum)
                {
                    Enum.TryParse(fieldInfo.PropertyType, value.ToString(), true, out var enumValue);
                    fieldInfo.SetValue(awsModel, enumValue);
                    continue;
                }

                fieldInfo.SetValue(awsModel, value);
            }

            return awsModel;
        }
    }
}