using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace KinectLetterParticelSwarm
{
   public class Word : Letter
   {
           

	//should word be shown right now?
	Boolean formActive;
	
	//Word as String
	String content;
	
	//size in which the word should be shown
	double size;
	
	//Letterobjects that the word is made of
	Letter[] containedLetters;
	
	//Color in which the word should be shown
	Color color;
	
    }
}
