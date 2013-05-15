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
        //public static List<HtmlDocument> ProcessExtraPage(HtmlDocument htmlDoc)
        //{
        //    var ret = new List<HtmlDocument>();
        //    if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
        //    {

        //    }
        //    else
        //    {
        //        if (htmlDoc.DocumentNode != null)
        //        {
        //            var searchPages = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='search_pagination_right']");
        //            if (searchPages != null)
        //            {
        //                var pages = searchPages.SelectNodes("a");
        //                if (pages != null)
        //                {
        //                    var pageUrls = new List<string>();
        //                    foreach (var page in pages)
        //                    {
        //                        pageUrls.Add(page.GetAttributeValue("href", ""));
        //                    }
        //                    pageUrls.RemoveAt(pageUrls.Count - 1);
        //                    var lastUrl = pageUrls[pageUrls.Count - 1];
        //                    var num = int.Parse(lastUrl.Substring(lastUrl.IndexOf("page=") + 5));
        //                    var url = "http://store.steampowered.com/search/?sort_by=&sort_order=ASC&specials=1&page=";
        //                    pageUrls.Clear();
        //                    if (num > 10)
        //                    {
        //                        // Only get 20 first pages.
        //                        num = 10;
        //                    }
        //                    for (var i = 2; i <= num; ++i)
        //                    {
        //                        pageUrls.Add(url + i.ToString());
        //                    }
                            
        //                    var extraDocs = new List<HtmlDocument>();
        //                    Parallel.ForEach(pageUrls, pu =>
        //                    {
        //                        var pageStr = "";
        //                        var wc = new WebClient();
        //                        try
        //                        {
        //                            pageStr = wc.DownloadString(pu);
        //                        }
        //                        catch (System.Net.WebException)
        //                        {
        //                        }
        //                        var currHtmlDoc = new HtmlDocument();
        //                        currHtmlDoc.LoadHtml(pageStr);
        //                        lock (ret)
        //                        {
        //                            ret.Add(currHtmlDoc);
        //                        }
        //                    });
                            
        //                }
        //            }
        //        }
        //    }
        //    return ret;
        //}

        public static void ProcessSearchDoc(HtmlDocument htmlDoc, List<SteamSpecialItem> retList)
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

        static SteamSpecialItem ParseNode(HtmlNode currNode)
        {
            var ret = new SteamSpecialItem();
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

                var cat_release = platformImg.NextSibling;
                if (cat_release != null)
                {
                    ret.Cat_Release = HtmlEntity.DeEntitize(cat_release.InnerText);
                }
            }
            catch (Exception e)
            {
            }
            if (string.IsNullOrEmpty(ret.OldPrice) || string.IsNullOrEmpty(ret.NewPrice))
            {
                ret.OldPrice = "$-1";
                ret.NewPrice = "$-1";
            }
            return ret;
        }
    }
}
