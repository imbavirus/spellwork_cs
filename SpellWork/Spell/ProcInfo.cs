﻿using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SpellWork.DBC.Structures;
using SpellWork.Extensions;

namespace SpellWork.Spell
{
    public class ProcInfo
    {
        public static SpellInfoHelper SpellProc { get; set; }
        public static bool Update = true;

        public ProcInfo(TreeView familyTree, SpellFamilyNames spellfamily)
        {
            familyTree.Nodes.Clear();

            var spells = from spell in DBC.DBC.SpellInfoStore.Values
                         where spell.SpellFamilyName == (uint)spellfamily
                         join sk in DBC.DBC.SkillLineAbility.Values on spell.ID equals sk.SpellID into temp1
                         from skill in temp1.DefaultIfEmpty(new SkillLineAbilityEntry())
                         join skl in DBC.DBC.SkillLine.Values on skill.SkillLine equals skl.Id into temp2
                         from skillLine in temp2.DefaultIfEmpty(new SkillLineEntry())
                         select new
                         {
                             spell,
                             skill.SkillLine,
                             skillLine
                         };

            for (var i = 0; i < 128; ++i)
            {
                var mask = new uint[4];

                if (i < 32)
                    mask[0] = 1U << i;
                else if (i < 64)
                    mask[1] = 1U << (i - 32);
                else if (i < 96)
                    mask[2] = 1U << (i - 64);
                else
                    mask[3] = 1U << (i - 96);

                var node   = new TreeNode
                {
                    Text = String.Format("0x{0:X8} {1:X8} {2:X8} {3:X8}", mask[3], mask[2], mask[1], mask[0]),
                    ImageKey = @"family.ico"
                };
                familyTree.Nodes.Add(node);
            }

            foreach (var elem in spells)
            {
                var spell = elem.spell;
                var isSkill = elem.SkillLine != 0;

                var name    = new StringBuilder();
                var toolTip = new StringBuilder();

                name.AppendFormat("{0} - {1} ", spell.ID, spell.SpellNameRank);

                toolTip.AppendFormatLine("Spell Name: {0}",  spell.SpellNameRank);
                toolTip.AppendFormatLine("Description: {0}", spell.Description);
                toolTip.AppendFormatLine("ToolTip: {0}",     spell.ToolTip);

                if (isSkill)
                {
                    name.AppendFormat("(Skill: ({0}) {1}) ", elem.SkillLine, elem.skillLine.Name);

                    toolTip.AppendLine();
                    toolTip.AppendFormatLine("Skill Name: {0}", elem.skillLine.Name);
                    toolTip.AppendFormatLine("Description: {0}", elem.skillLine.Description);
                }

                name.AppendFormat("({0})", spell.School.ToString().NormalizeString("MASK_"));

                foreach (TreeNode node in familyTree.Nodes)
                {
                    var mask = new uint[4];

                    if (node.Index < 32)
                        mask[0] = 1U << node.Index;
                    else if (node.Index < 64)
                        mask[1] = 1U << (node.Index - 32);
                    else if (node.Index < 96)
                        mask[2] = 1U << (node.Index - 64);
                    else
                        mask[3] = 1U << (node.Index - 96);

                    if ((!spell.SpellFamilyFlags.ContainsElement(mask)))
                        continue;

                    var child       = node.Nodes.Add(name.ToString());
                    child.Name      = spell.ID.ToString();
                    child.ImageKey  = isSkill ? "plus.ico" : "munus.ico";
                    child.ForeColor = isSkill ? Color.Blue : Color.Red;
                    child.ToolTipText = toolTip.ToString();
                }
            }
        }
    }
}
