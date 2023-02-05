// Old Skull Games
// Bernard Barthelemy
// Friday, April 26, 2019

using System;
using System.Collections.Generic;

namespace OSG.ConfigHelper
{
    [Serializable]
    internal class Inclusion
    {
        public string elementGUID;
        public Inclusion(Element element)
        {
            elementGUID = element.guid;
        }

        public bool overrideArchivePath;

        public void Include(List<Element> elementsToProcess, Config config)
        {
            for(int i = elementsToProcess.Count;--i>=0;)
            {
                if(elementsToProcess[i].guid == elementGUID)
                {
                    elementsToProcess[i].Include(config.GetArchiveAbsolutePath(elementsToProcess[i], overrideArchivePath));
                    elementsToProcess.RemoveAt(i);
                    break;
                }
            }
        }

    }
}