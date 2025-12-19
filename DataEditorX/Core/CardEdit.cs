using DataEditorX.Language;
using System.Collections.Generic;

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
            if (!dataform.IsFileExists())
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
            undoSQL = Database.GetDeleteSQL(c);
            dataform.Refresh(true);
            dataform.LoadCard(c);
            return true;
        }

        public bool UpdateCommand(bool sync)
        {
            if (!dataform.IsFileExists())
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
            if (!Database.UpdateCard(dataform.GetOpenFile(), c, oldCard))
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
            if (!dataform.IsFileExists())
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
            if (!dataform.IsFileExists())
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
                int count = 0;
                foreach (Card oc in oldcards)
                {
                    foreach (Card c in cards)
                    {
                        if (c.id == oc.id)
                        {
                            count += 1;
                            if (count >= 1)
                            {
                                replace = MyMsg.Question(LMSG.IfReplaceExistingCard);
                                break;
                            }
                        }
                    }
                    if (count >= 1)
                    {
                        break;
                    }
                }
            }
            Database.InsertCards(dataform.GetOpenFile(), !replace, cards);
            dataform.Refresh(true);
            return true;
        }
    }
}
