using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SteamSpecialsWp.ViewModel
{
    public static class SSParser
    {
        public static int ParseNumberOfPages(HtmlDocument htmlDoc)
        {
            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
            {
                return -1;
            }
            else
            {
                if (htmlDoc.DocumentNode != null)
                {
                    var searchPages = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='search_pagination_right']");
                    if (searchPages != null)
                    {
                        var pages = searchPages.SelectNodes("a");
                        if (pages != null)
                        {
                            var pageUrls = new List<string>();
                            foreach (var page in pages)
                            {
                                pageUrls.Add(page.GetAttributeValue("href", ""));
                            }
                            pageUrls.RemoveAt(pageUrls.Count - 1);
                            var lastUrl = pageUrls[pageUrls.Count - 1];
                            var num = int.Parse(lastUrl.Substring(lastUrl.IndexOf("page=") + 5));

                            return num;
                        }
                    }
                }
            }
            return -1;
        }

        public static string ParseInfoText(HtmlDocument htmlDoc)
        {
            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
            {
                return "error";
            }
            else
            {
                if (htmlDoc.DocumentNode != null)
                {
                    var iText = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='search_pagination_left']");
                    if (iText != null)
                    {
                        var str = HtmlEntity.DeEntitize(iText.InnerText);
                        str = str.Trim();
                        return str;
                    }
                }
            }
            return "error";
        }

        public static void ParseDealPage(HtmlDocument htmlDoc, ICollection<SteamSpecialItemViewModel> retList)
        {
            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
            {
                return;
            }
            if (htmlDoc.DocumentNode != null)
            {
                var searchResults = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='search_results']");

                if (searchResults != null)
                {
                    var even = searchResults.SelectNodes("//a[@class='search_result_row even']");
                    var odd = searchResults.SelectNodes("//a[@class='search_result_row odd']");
                    
                    if (even != null && odd != null)
                    {
                        var max = Math.Max(even.Count, odd.Count);
                        for (var i = 0; i < max; ++i)
                        {
                            if (i < even.Count)
                            {
                                retList.Add(ParseNode(even[i]));
                            }
                            if (i < odd.Count)
                            {
                                retList.Add(ParseNode(odd[i]));
                            }
                        }
                    }
                }
            }
        }

        static SteamSpecialItemViewModel ParseNode(HtmlNode currNode)
        {
            var ret = new SteamSpecialItemViewModel();
            try
            {
                var name = currNode.SelectSingleNode("div[@class='col search_name ellipsis']/h4");
                if (name != null)
                {
                    ret.Name = HtmlEntity.DeEntitize(name.InnerText);
                }

                var imgLink = currNode.SelectSingleNode("div[@class='col search_capsule']/img");
                if (imgLink != null)
                {
                    ret.ImgUrl = imgLink.GetAttributeValue("src", "nolink");
                }

                var link = currNode.GetAttributeValue("href", "nolink");
                ret.Link = link;

                var oldPrice = currNode.SelectSingleNode("div[@class='col search_price']/span/strike");
                if (oldPrice != null)
                {

                    ret.OldPrice = HtmlEntity.DeEntitize(oldPrice.InnerText);
                }

                var newPrice = currNode.SelectSingleNode("div[@class='col search_price']/text()");
                if (newPrice != null)
                {
                    ret.NewPrice = HtmlEntity.DeEntitize(newPrice.InnerText);
                }

                var metaScore = currNode.SelectSingleNode("div[@class='col search_metascore']");
                if (metaScore != null)
                {
                    ret.MetaScore = HtmlEntity.DeEntitize(metaScore.InnerText);
                }

                var typeImg = currNode.SelectSingleNode("div[@class='col search_type']/img");
                if (typeImg != null)
                {
                    ret.TypeImg = typeImg.GetAttributeValue("src", "nolink");
                }

                var platformImg = currNode.SelectSingleNode("div[@class='col search_name ellipsis']/p/img");
                if (platformImg != null)
                {
                    ret.PlatformImg = platformImg.GetAttributeValue("src", "nolink");
                }

                var cat_release = currNode.SelectSingleNode("div[@class='col search_name ellipsis']/p");
                if (cat_release != null)
                {
                    ret.Cat_Release = HtmlEntity.DeEntitize(cat_release.InnerText).Trim();
                }
            }
            catch (Exception e)
            {
            }
            if (string.IsNullOrEmpty(ret.OldPrice) || string.IsNullOrEmpty(ret.NewPrice))
            {
                ret.OldPrice = null;
                ret.NewPrice = null;
            }
            return ret;
        }
    }
}
