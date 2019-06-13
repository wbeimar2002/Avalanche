using IsmDmag;
using IsmRec.Types;

namespace MonsoonAPI.models
{

    public class MovieMonsoon : DmagM.DmagPathM
    {
        public string PosterFrame;
        public double Length;

        public MovieMonsoon()
        {
        }

        /// <summary>
        /// Init object for library procedure
        /// </summary>
        public MovieMonsoon(clsMovie mov) : base(mov)
        {
            PosterFrame = mov.ThumbName;
            Length = mov.Length;
        }

        /// <summary>
        /// Init objectr for active procedure
        /// </summary>
        public MovieMonsoon(VideoData video) : base(video)
        {
            PosterFrame = clsPicture.GetThumbnailFileName(RelativePath);
            Length = video.m_dSeconds;
        }
    }
}
