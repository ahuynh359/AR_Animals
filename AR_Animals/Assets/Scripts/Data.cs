using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Vuforia;
using System;

public class Data : MonoBehaviour
{


    public Text nameObject;
    public Text infoName;
    public Button soundButton;
    public Button noSoundButton;
    public Button nameButton;
    public Button closeInfo;
    public Button openInfo;
    public GameObject infoPanel;

    private AudioSource audios;
    private AudioClip clips;
    public static Data instance;
    private static String nameTrack;

    private List<string> names = new List<string> {"Con Bướm","Con Cá Voi","Con mèo","Con chim cánh cụt",
        "Con Voi", "Con Chim", "Con Nhện","Con Cá Mồi","Con ngựa", "Con Hổ","Con Tê Giác","Ốc Anh Vũ","Kì Nhông","Khỉ Đột","Khủng Long" };

    private List<string> infos = new List<string> {
"Là các loài côn trùng nhỏ, biết bay, hoạt động vào ban ngày chúng sống gần các bụi cây nhiều hoa để hút phấn hoa, mật hoa.",
"Là một loài cá voi lớn,  thường xuyên nhảy vọt lên khỏi mặt nước. Cá voi lưng gù chỉ ăn ở vùng cực vào mùa hè. Cơ thể của chúng có dạng tựa hình thoi.",
"Mèo giao tiếp bằng cách kêu meo. Đa số các giống mèo đều thích trèo cao hay ngồi ở các vị trí cao. Mèo được biết đến nhờ sự sạch sẽ của nó khả năng săn bắt hàng nghìn loại sinh vật để làm thức ăn.",
"Là một bộ chim không cánh sinh sống dưới nước là chủ yếu tại khu vực toàn băng tuyết. Chúng có lông rậm, mỡ dày để chịu rét. ",
"Voi là loài động vật lớn nhất sinh sống trên mặt đất. Voi có bốn chân lớn để có thể chịu được trọng lượng cơ thể nặng. Ngà voi là do hai răng cửa hàm trên biến thành.",
"Là các loài động vật có có cánh biết bay, sống khắp toàn cầu. Chim là động vật sống bầy đàn, chúng giao tiếp với nhau thông qua tiếng kêu và tiếng hót.",
"Nhện là một bộ động vật săn mồi, có khả năng làm màng nhện và tiêm nọc độc khi cắn để tự vệ và để giết mồi. Cơ thể chỉ có hai phần, tám chân, miệng không hàm nhai, không cánh.",
"Di cư thành từng đàn lớn  để chống lại kẻ thù săn mồi. Ở vùng nước ngọt chúng đẻ trứng nhưng khi trưởng thành chúng lại bơi ra biển và bước vào hành trình sinh sống ở đại dương bao la.",
"Ngựa là động vật ăn cỏ thường sống thành bầy, có trí nhớ dài hạn vô cùng tuyệt vời. Ngựa thường dùng để cưỡi, kéo xe, thồ hàng, làm ngựa chiến, ngựa đua.",
"Phần lớn các loài hổ sống trong rừng và đồng cỏ. Thức ăn của chúng chủ yếu là các động vật ăn cỏ cỡ trung bình như hươu, nai, lợn rừng, trâu và thường đi săn đơn lẻ.",
"Tê giác chỉ ăn cỏ, các loại lá cây, cành và chồi non sống ở vùng đất thấp, vùng đồng cỏ ẩm ướt và các bãi bồi triền sông rộng lớn.",
"Sống dưới đáy biển sâu vài trăm mét, có chiếc vỏ cứng rất đẹp, bên ngoài có hình lượn sóng xám đỏ xen nhau, bên trong là lớp màu trắng bạc long lanh.",
"Sống trên đất cát ven biển, rất thích có bóng mát. Nguồn thức ăn chủ yếu là thức ăn thực vật.Trước mùa đông, kỳ nhông thường thu thức ăn về để ở dưới hang.",
"Động vật ăn cỏ sống trong rừng. Thức ăn chủ yếu của chúng là các loại thực vật như cây mọng nước, chồi non,..Khỉ đột sống theo đàn. Đi bằng bốn chân và chỉ đi bằng hai chân khi chuẩn bị đánh nhau.",
"Đây là loài khủng long đã tuyệt chủng có 2 chân. Khủng long có mặt ở khắp các châu lục, qua những hóa thạch còn sót lại. Đây là loài khủng long ăn cỏ. ",
 };



    void Start()
    {

        audios = gameObject.AddComponent<AudioSource>();
        if (instance == null)
            instance = this;
    }


    // Update is called once per frame
    void Update()
    {

        IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetActiveTrackableBehaviours();

        foreach (TrackableBehaviour tb in tbs)
        {
            nameTrack = tb.TrackableName;

            int index = Convert.ToInt32(nameTrack);
            nameObject.gameObject.GetComponent<Text>().text = names[index];

            if (nameTrack == "6" || nameTrack == "7" || nameTrack == "11" || nameTrack == "12")
            {
                _EnableSoundButton(false);
            }
            else
            {
                _EnableSoundButton(true);
                soundButton.GetComponent<Button>().onClick.AddListener(delegate { _LoadSound("Sound/" + nameTrack); });
            }

            infoName.GetComponent<Text>().text = infos[index];
            nameButton.GetComponent<Button>().onClick.AddListener(delegate { _LoadSound("NameSound/" + nameTrack); });

            openInfo.GetComponent<Button>().onClick.AddListener(delegate { _EnableInfo(false); });
            closeInfo.GetComponent<Button>().onClick.AddListener(delegate { _EnableInfo(true); });


        }


    }




    private void _LoadSound(string name)
    {
        clips = (AudioClip)Resources.Load(name);
        audios.clip = clips;
        audios.playOnAwake = false;
        audios.loop = false;
        audios.Play();
    }

    private void _EnableSoundButton(bool b)
    {
        if (b)
        {
            soundButton.gameObject.SetActive(true);
            noSoundButton.gameObject.SetActive(false);

        }
        else
        {
            soundButton.gameObject.SetActive(false);
            noSoundButton.gameObject.SetActive(true);

        }
    }

    private void _EnableInfo(bool b)
    {

        if (b)
        {
            openInfo.gameObject.SetActive(true);
            infoPanel.SetActive(false);
            closeInfo.gameObject.SetActive(false);
        }
        else
        {
            openInfo.gameObject.SetActive(false);
            infoPanel.gameObject.SetActive(true);
            closeInfo.gameObject.SetActive(true);
        }
    }

    public string _GetName()
    {
        return nameTrack;
    }
}
