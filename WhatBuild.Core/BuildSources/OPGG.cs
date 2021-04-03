using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.Enums;
using WhatBuild.Core.Interfaces;
using WhatBuild.Core.Utils;

namespace WhatBuild.Core.BuildSources
{
    /// <summary>
    /// Will fetch the most popular position build
    /// </summary>
    /// <see cref="https://op.gg"/>
    public class OPGG : IBuildSource
    {
        private string BaseUrl => "https://www.op.gg/champion/";

        private HtmlDocument Document { get; set; }

        public async Task ReadHtmlDocumentAsync(string championName)
        {
            string championUrl = BaseUrl + championName;

            HtmlWeb web = new HtmlWeb();

            // TODO/v2: Handle multiple positions
            // Right now, 1 document is assigned to one champion per popular position
            Document = await web.LoadFromWebAsync(championUrl);
        }

        #region Positions
        public ChampionPosition GetChampionPosition()
        {
            // Select the most popular position (first)
            // TODO/v2: Store XPath somewhere in Github Gist so that it gets recent xpath without updating software
            HtmlNode nodePosition = Document.DocumentNode.SelectSingleNode("//*[@class='champion-stats-position']/li");
            if (nodePosition == null)
            {
                throw new NullReferenceException("[OP.GG] Node position was not found");
            }

            string position = nodePosition.Attributes["data-position"]?.Value;

            return ChampionPositionUtil.Parse(position);
        }
        #endregion

        #region Skills
        /// <summary>
        /// Returns the 4 first skills to level up, and general skills right next to it
        /// Example: "W.Q.E.Q [Q -> W -> E]"
        /// </summary>
        public string GetFormattedSkills()
        {
            string firstSkills = GetFirstSkillsOrder();
            string generalSkills = GetGeneralSkillsOrder();

            return $"{firstSkills} [{generalSkills}]";
        }

        /// <summary>
        /// Example: Q -> W -> E
        /// </summary>
        private string GetGeneralSkillsOrder()
        {
            HtmlNodeCollection nodes = Document.DocumentNode.SelectNodes("//*[contains(@class, 'champion-stats__list__item tip')]/span");

            if (nodes.Count != 3)
            {
                throw new InvalidOperationException("General skills are only composed of 3 main skills");
            }

            return $"{nodes[0].InnerText} -> {nodes[1].InnerText} -> {nodes[2].InnerText}";
        }

        /// <summary>
        /// Returns the 4 first skills to level up
        /// Example: W.Q.E.Q
        /// </summary>
        private string GetFirstSkillsOrder()
        {
            HtmlNodeCollection nodes = Document.DocumentNode.SelectNodes("//table[@class='champion-skill-build__table']/tbody/tr[2]/td");

            if (nodes.Count < 4)
            {
                throw new InvalidOperationException($"There should be at least 4 first skills to level");
            }

            string formattedFirstSkills =
                $"{nodes[0].InnerText}.{nodes[1].InnerText}.{nodes[2].InnerText}.{nodes[3].InnerText}";

            // We need to clean string (full of \n and \t)
            formattedFirstSkills = formattedFirstSkills
                .Replace("\t", string.Empty)
                .Replace("\n", string.Empty);

            return formattedFirstSkills;
        }

        #endregion

        #region Item builds

        #region Starter Items
        #endregion

        #region Core Items
        #endregion

        #region Boots
        #endregion

        #endregion
    }
}
