import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import org.apache.http.HttpEntity;
import org.apache.http.client.methods.CloseableHttpResponse;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.entity.ContentType;
import org.apache.http.entity.mime.MultipartEntityBuilder;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.HttpClients;

import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;

public class Main {
    public final static String SAMPLE_IMAGE_PATH = "../Images";

    public static void main (String args[]) throws IOException {

        ImageSet imageSet = new ImageSet();
        imageSet.name = "test";
        imageSet.images = new ArrayList<Image>();

        File dir = new File(SAMPLE_IMAGE_PATH);
        for(File file : dir.listFiles()){

            if(!file.getName().endsWith(".jpg") && !file.getName().endsWith(".png"))
                continue;

            Path path = Paths.get(file.getPath());

            Image image = new Image();
            image.fileName = file.getName();
            image.mimeType = Files.probeContentType(path);
            image.imageData = Files.readAllBytes(path);
			image.side = SideEnum.Front;
			
            imageSet.images.add(image);
        }

        postImageSet(imageSet);
    }

    private static void postImageSet(ImageSet imageSet) throws IOException {

        MultipartEntityBuilder builder = MultipartEntityBuilder.create();

        Gson gson = new GsonBuilder()
                .excludeFieldsWithoutExposeAnnotation()
                .create();

        String imageSetJson = gson.toJson(imageSet);
        builder.addTextBody("imageset", imageSetJson, ContentType.APPLICATION_JSON);

        int counter = 0;
        for(Image image : imageSet.images) {
            builder.addBinaryBody(
                    "image" + counter++,
                    image.imageData,
                    ContentType.create(image.mimeType), image.fileName);
			builder.addPart("Side", image.Side);
        }

        HttpEntity multipartHttpEntity = builder.build();

        HttpPost httpPost = new HttpPost("http://93.153.125.236/Impresto.Ocr/api/recognize");
        httpPost.setEntity(multipartHttpEntity);

        CloseableHttpClient httpclient = HttpClients.createDefault();

        try {
            CloseableHttpResponse response = httpclient.execute(httpPost);
            response.getEntity().writeTo(System.out);
            response.close();
        }
        finally {
            httpclient.close();
        }
    }
}
