using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavidSpeck.CSharpDocOutline.CDM
{
	public static class ParserUtilities
	{
		/// <summary>
		/// Split a string into words by spliting along spaces but ignoring spaces in between of < and >
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string[] GetWords(string str)
		{
			string post = str;
			string pre = "";
			string sub = "";

			// Index of '<'
			int indexOfOpen = -1;
			// Index of '>'
			int indexOfClosing = -1;
			while((indexOfOpen = post.IndexOf('<')) >= 0){
				indexOfClosing = post.IndexOf('>', indexOfOpen);

				// Remove space inbetween '<' and '>'
				sub = post.Substring(indexOfOpen, indexOfClosing - indexOfOpen);
				sub = sub.Replace(" ", "");

				sub = post.Substring(0, indexOfOpen) + sub;

				// Add cleaned part to 'pre's and remove it from 'post'
				pre += sub;
				post = post.Remove(0, sub.Length);
			}

			// Add the rest of 'post' to 'pre'
			pre += post;

			// Split them by spaces
			string[] words = pre.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			return words;
		}
	}
}
