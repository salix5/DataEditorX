using System.Collections.Generic;
using System.Linq;
using DataEditorX.Language;

namespace DataEditorX.Core
{
    public class CardEdit
    {
        readonly IDataForm dataform;

        public CardEdit(IDataForm dataform)
        {
            this.dataform = dataform;
        }

        public bool AddCommand()
        {
            if (!dataform.IsOpened())
            {
                return false;
            }
            Card c = dataform.GetCard();
            if (c.id <= 0)
            {
                MyMsg.Error(LMSG.InvalidCode);
                return false;
            }
            if (!Database.AddCard(dataform.GetOpenFile(), c))
            {
                MyMsg.Warning(LMSG.ItIsExists);
                return false;
            }
            MyMsg.Show(LMSG.AddSucceed);
            dataform.Refresh(true);
            dataform.LoadCard(c);
            return true;
        }

        public bool UpdateCommand(bool sync)
        {
            if (!dataform.IsOpened())
            {
                return false;
            }
            Card c = dataform.GetCard();
            Card oldCard = dataform.GetOldCard();
            if (c.Equals(oldCard))
            {
                MyMsg.Show(LMSG.ItIsNotChanged);
                return false;
            }
            if (c.id <= 0)
            {
                MyMsg.Error(LMSG.InvalidCode);
                return false;
            }
            long oldId = 0;
            if (c.id != oldCard.id)
            {
                oldId = oldCard.id;
                if (sync)
                {
                    YGOUtil.CardRename(c.id, oldCard.id, dataform.GetPath());
                }
            }
            if (!Database.UpdateCard(dataform.GetOpenFile(), c, oldId))
            {
                MyMsg.Error(LMSG.ModifyFail);
                return false;
            }
            MyMsg.Show(LMSG.ModifySucceed);
            dataform.Refresh(true);
            dataform.LoadCard(c);
            return true;
        }

        public bool DeleteCommand(bool sync)
        {
            if (!dataform.IsOpened())
            {
                return false;
            }
            Card[] cards = dataform.GetCardList(true);
            if (cards.Length == 0)
            {
                return false;
            }
            if (!MyMsg.Question(LMSG.IfDeleteCard))
            {
                return false;
            }
            if (sync)
            {
                foreach (Card c in cards)
                {
                    YGOUtil.CardDelete(c.id, dataform.GetPath());
                }
            }
            if (Database.DeleteCards(dataform.GetOpenFile(), cards) < cards.Length * 2)
            {
                MyMsg.Error(LMSG.DeleteFail);
                dataform.Refresh(true);
                return false;
            }
            MyMsg.Show(LMSG.DeleteSucceed);
            dataform.Refresh(true);
            return true;
        }

        public bool CopyCommand(Card[] cards)
        {
            if (!dataform.IsOpened())
            {
                return false;
            }
            if (cards is null || cards.Length == 0)
            {
                return false;
            }
            bool replace = false;
            Card[] oldcards = Database.Read(dataform.GetOpenFile(), "");
            if (oldcards.Length > 0)
            {
                HashSet<long> newCardIds = new(cards.Select(c => c.id));
                bool hasDuplicate = oldcards.Any(oc => newCardIds.Contains(oc.id));
                if (hasDuplicate)
                {
                    replace = MyMsg.Question(LMSG.IfReplaceExistingCard);
                }
            }
            Database.InsertCards(dataform.GetOpenFile(), !replace, cards);
            dataform.Refresh(true);
            return true;
        }
    }
}
