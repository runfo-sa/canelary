namespace Editor.Models
{
    public static class ResizeZPL
    {
        /*
         * Scales text from a raw ZPL label from 203 DPI to 300 DPI
         */

        public static string Resize(string rawCommands, float? scaleFactor)
        {
            // if there are no ZPL commands, return the same.
            if (!rawCommands.Contains("^"))
                return rawCommands;

            // ZPL commands to be handled. Other commands remain intact.
            // key is the command name, value is the maximum number of parameters to process.
            // if null all parameters will be scaled.
            Dictionary<string, int?> cmds = new Dictionary<string, int?> {
                {"FO", 2},
                {"PW", null},
                {"FT", 2},
                {"A0", null},
                {"A1", null},
                {"A2", null},
                {"A3", null},
                {"A4", null},
                {"A5", null},
                {"A6", null},
                {"A7", null},
                {"A8", null},
                {"A9", null},
                {"A@", null},
                {"LL", null},
                {"LH", null},
                {"GB", null}, // 5th parameter has special handling, see scaleSection().
                {"FB", null},
                {"BY", null}, // 1st and 2nd parameters have special handling, see scaleSection().
                {"BQ", 3}, // 3rd parameter has special handling, see scaleSection().
                {"B3", null},
                {"BC", null},
                {"B7", 2}
            };
            if (scaleFactor == null)
            {
                scaleFactor = 1.5f; // assuming scaling from 203 dpi to 300 dpi, i.e. 8dpi to 12dpi. 300f / 203;
            }

            var sections = rawCommands.Split('^');
            foreach (var cmd in cmds)
            {
                for (int j = 0; j < sections.Length; ++j)
                {
                    if (sections[j].StartsWith(cmd.Key))
                    {
                        sections[j] = ScaleSection(cmd, sections[j], scaleFactor ?? 1);
                    }
                }
            }

            return string.Join("^", sections);
        }

        /*
         * Scales all integers found in a designated section
         */

        private static string ScaleSection(KeyValuePair<string, int?> cmd, string section, float scaleFactor)
        {
            string[] parts = section.Substring(cmd.Key.Length).Split(',');
            for (int p = 0; p < parts.Length; ++p)
            {
                float f;
                if (float.TryParse(parts[p], out f) && p < (cmd.Value ?? 999))
                {
                    double newValue = Math.Round(scaleFactor * f, MidpointRounding.ToZero);

                    if (cmd.Key == "BY")
                    {
                        if (p == 0)
                        { // module width (in dots) Values: 1 to 10
                            if (newValue < 1)
                                newValue = 1;
                            else if (newValue > 10)
                                newValue = 10;
                        }
                        else if (p == 1)
                        { // wide bar to narrow bar width ratio Values: 2.0 to 3.0, in 0.1 increments
                            continue; // do not scale this part
                        }
                    }
                    else if (cmd.Key == "BQ")
                    {
                        if (p == 2)
                        { // magnification factor Values: 1 to 10
                            if (newValue < 1)
                                newValue = 1;
                            else if (newValue > 10)
                                newValue = 10;
                        }
                        else
                            continue; // do not scale other parts of BQ.
                    }
                    else if (cmd.Key == "GB" && p == 4 && newValue > 8)
                    { // degree of corner rounding : 0(no rounding) to 8(heaviest rounding)
                        newValue = 8;
                    }
                    parts[p] = newValue.ToString();
                }
            }

            return cmd.Key + string.Join(",", parts);
        }
    }
}
