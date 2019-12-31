﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using tweetz.core.Commands;
using tweetz.core.Controls.UserProfileBlock;
using tweetz.core.Models;
using twitter.core.Models;

namespace tweetz.core.Services
{
    public static partial class FlowContentService
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            "Source",
            typeof(TwitterStatus),
            typeof(FlowContentService),
            new PropertyMetadata(null, OnSourceChanged));

        public static TwitterStatus GetSource(DependencyObject d)
        {
            return (TwitterStatus)d.GetValue(SourceProperty);
        }

        public static void SetSource(DependencyObject d, TwitterStatus value)
        {
            d.SetValue(SourceProperty, value);
        }

        private static void OnSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs ea)
        {
            var textBlock = (TextBlock)sender;
            var twitterStatus = (TwitterStatus)ea.NewValue;
            textBlock.Inlines.Clear();
            textBlock.Inlines.AddRange(SourceToFlowContentInlines(twitterStatus));
        }

        public static IEnumerable<object> SourceToFlowContentInlines(TwitterStatus twitterStatus)
        {
            if (twitterStatus == null)
            {
                yield break;
            }

            var nodes = BuildFlowContentNodes(twitterStatus);

            foreach (var node in nodes)
            {
                switch (node.FlowContentNodeType)
                {
                    case FlowContentNodeType.Text:
                        yield return Run(node.Text);
                        break;

                    case FlowContentNodeType.Url:
                        yield return Link(node.Text);
                        break;

                    case FlowContentNodeType.Mention:
                        yield return Mention(node.Text);
                        break;

                    case FlowContentNodeType.HashTag:
                        yield return Hashtag(node.Text);
                        break;

                    case FlowContentNodeType.Media:
                        continue;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static Run Run(string text)
        {
            return new Run(ConvertXmlEntities(text));
        }

        private static InlineUIContainer Link(string link)
        {
            var hyperlink = new Hyperlink(new Run(link))
            {
                Command = OpenLinkCommand.Command,
                CommandParameter = link,
                ToolTip = link,
            };

            var textblock = new TextBlock(hyperlink)
            {
                MaxWidth = 150,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            var container = new InlineUIContainer(textblock);
            return container;
        }

        private static Hyperlink Mention(string text)
        {
            var tooltip = new ToolTip();
            var userProfile = new UserProfileBlock();

            tooltip.Content = userProfile;
            userProfile.Tag = text;

            return new Hyperlink(new Run("@" + text))
            {
                Command = OpenLinkCommand.Command,
                CommandParameter = $"https://twitter.com/{text}",
                ToolTip = tooltip
            };
        }

        private static Hyperlink Hashtag(string text)
        {
            var tag = "#" + text;
            return new Hyperlink(new Run(tag))
            {
                Command = SearchCommand.Command,
                CommandParameter = tag
            };
        }

        private static string ConvertXmlEntities(string text)
        {
            return text
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&quot;", "\"")
                .Replace("&apos;", "'")
                .Replace("&amp;", "&");
        }

        private static IEnumerable<FlowContentNode> BuildFlowContentNodes(TwitterStatus twitterStatus)
        {
            var start = 0;
            var twitterString = new TwitterString(twitterStatus.FullText ?? twitterStatus.Text ?? string.Empty);
            var flowContentItems = BuildFlowControlItems(twitterStatus.Entities ?? new Entities());

            foreach (var item in flowContentItems)
            {
                if (item.Start >= start)
                {
                    var len = item.Start - start;
                    yield return new FlowContentNode(FlowContentNodeType.Text, twitterString.Substring(start, len));
                }

                yield return new FlowContentNode(item.FlowContentNodeType, item.Text);
                start = item.End;
            }

            yield return new FlowContentNode(FlowContentNodeType.Text, twitterString.Substring(start));
        }

        private static List<FlowContentItem> BuildFlowControlItems(Entities entities)
        {
            var flowContentItems = new List<FlowContentItem>();

            if (entities.Urls != null)
            {
                flowContentItems.AddRange(entities.Urls
                    .Select(url => new FlowContentItem
                    {
                        FlowContentNodeType = FlowContentNodeType.Url,
                        Text = url.ExpandedUrl,
                        Start = url.Indices[0],
                        End = url.Indices[1]
                    }));
            }

            if (entities.Mentions != null)
            {
                flowContentItems.AddRange(entities.Mentions
                    .Select(mention => new FlowContentItem
                    {
                        FlowContentNodeType = FlowContentNodeType.Mention,
                        Text = mention.ScreenName,
                        Start = mention.Indices[0],
                        End = mention.Indices[1]
                    }));
            }

            if (entities.HashTags != null)
            {
                flowContentItems.AddRange(entities.HashTags
                    .Select(hashtag => new FlowContentItem
                    {
                        FlowContentNodeType = FlowContentNodeType.HashTag,
                        Text = hashtag.Text,
                        Start = hashtag.Indices[0],
                        End = hashtag.Indices[1]
                    }));
            }

            if (entities.Media != null)
            {
                flowContentItems.AddRange(entities.Media
                    .Select(media => new FlowContentItem
                    {
                        FlowContentNodeType = FlowContentNodeType.Media,
                        Text = media.Url,
                        Start = media.Indices[0],
                        End = media.Indices[1]
                    }));
            }

            flowContentItems.Sort((l, r) => l.Start - r.Start);
            return flowContentItems;
        }
    }
}