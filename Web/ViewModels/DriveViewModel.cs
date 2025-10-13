namespace Web.ViewModels
{
    public class BreadcrumbItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class FolderViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FileViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string MimeType { get; set; }
        public DateTime UploadedAt { get; set; }
        public string Icon { get; set; }
    }

    public class DriveViewModel
    {
        public Guid CurrentFolderId { get; set; }
        public string CurrentFolderName { get; set; }
        public List<FolderViewModel> Folders { get; set; }
        public List<FileViewModel> Files { get; set; }
        public List<BreadcrumbItem> Breadcrumbs { get; set; }
    }

}
