/*
 * Created by SharpDevelop.
 * User: cruzl
 * Date: 4/26/2026
 * Time: 1:03 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace cwtsGame
{
	/// <summary>
	/// Description of InventorySlot.
	/// </summary>
	public class InventorySlot
	{
		public TrashItem Item;
        public bool IsEmpty
        {
            get { return Item == null; }
        }

        public InventorySlot()
        {
            Item = null;
        }
	}
}
