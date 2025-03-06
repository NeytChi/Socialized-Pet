﻿using Core.FileControl;
using Domain.AutoPosting;
using Serilog;
using UseCases.AutoPosts.AutoPostFiles.Commands;
using UseCases.Exceptions;

namespace UseCases.AutoPosts.AutoPostFiles
{
    public class AutoPostFileManager : BaseManager, IAutoPostFileManager
    {
        private IAutoPostRepository AutoPostRepository;
        private IAutoPostFileRepository AutoPostFileRepository;
        private IAutoPostFileSave AutoPostFileSave;
       
        public AutoPostFileManager(ILogger logger,
            IAutoPostRepository autoPostRepository,
            IFileManager fileManager,
            IAutoPostFileRepository autoPostFileRepository,
            IAutoPostFileSave autoPostFileSave) : base(logger)
        {
            AutoPostRepository = autoPostRepository;
            AutoPostFileRepository = autoPostFileRepository;
            AutoPostFileSave = autoPostFileSave;
        }
        public ICollection<AutoPostFile> AddRange(AddRangeAutoPostFileCommand command)
        {
            var post = AutoPostRepository.GetByWithUserAndFiles(command.UserToken, command.AutoPostId);
            if (post == null)
            {
                throw new NotFoundException($"Сервер не визначив авто-пост по id={command.AutoPostId}.");
            }
            //if (!CheckFiles(cache.files, ref message))
            {
            //    return null;
            }
            if ((post.Files.Count() + command.Files.Count) > 10)
            {
                throw new ValidationException("Для автопосту дозволено лише 10 файлів.");
            }
            var postFiles = Create(command.Files, post, (sbyte)(post.Files.Count() + 1));
            foreach (var file in postFiles)
            {
                file.PostId = post.Id;
            }
            AutoPostFileRepository.Create(postFiles);
            return postFiles;
        }
        public void Update(ICollection<UpdateAutoPostFileCommand> commandFiles, ICollection<AutoPostFile> autoPost)
        {
            foreach (var file in commandFiles)
            {
                var exist = autoPost.Where(f => f.Id == file.Id).FirstOrDefault();
                if (exist == null)
                {
                    throw new SystemValidationException("Відправлені file id не відповідають file id на сервері.");
                }
                else
                {
                    exist.Order = file.Order;
                }
            }
        }
        public void Delete(DeleteAutoPostFileCommand command)
        {
            var post = AutoPostRepository.GetByWithFiles(command.AutoPostId);
            if (post == null)
            {
                throw new NotFoundException($"Сервер не визначив файл по id={command.AutoPostId} для видалення.");
            }
            if (post.Files.Count == 1)
            {
                post.Deleted = true;
                AutoPostRepository.Update(post);
                Logger.Information($"Авто пост був видалений id={post.Id}, тому що були видалені всі файли.");
                return;
            }
            var file = post.Files.Where(f => f.Id == command.AutoPostId).First();
            file.IsDeleted = true;
            foreach (var oldFile in post.Files)
            {
                if (oldFile.Order > file.Order)
                {
                    --oldFile.Order;
                }
            }
            AutoPostFileRepository.Update(file);
            AutoPostFileRepository.Update(post.Files);
            Logger.Information($"Файл був видалений з автопосту, файл id={file.Id}.");
        }
        public ICollection<AutoPostFile> Create(ICollection<CreateAutoPostFileCommand> files, AutoPost post , sbyte startOrder)
        {
            var postFiles = new List<AutoPostFile>();

            foreach (var file in files)
            {
                var autoPostFile = new AutoPostFile
                {
                    Path = "",
                    MediaId = "",
                    VideoThumbnail = "",
                    Post = post,
                    Type = file.FormFile.ContentType.Contains("video"),
                    Order = startOrder++,
                    CreatedAt = DateTime.UtcNow
                };
                if (autoPostFile.Type)
                {
                    if (!AutoPostFileSave.CreateVideoFile(autoPostFile, file.FormFile))
                    {
                        throw new IgAccountException("Сервер не зміг зберегти відео файл.");
                    }
                }
                else
                {
                    if (!AutoPostFileSave.CreateImageFile(autoPostFile, file.FormFile))
                    {
                        throw new IgAccountException("Сервер не зміг зберегти зображення.");
                    }
                }
                postFiles.Add(autoPostFile);
            }
            return postFiles;
        }
    }
}
