interface NotificationItem {
  id:string;
  avatar: string;
  date: string;
  isRead?: boolean;
  message: string;
  title: string;
  url: string;
}

export type { NotificationItem };
