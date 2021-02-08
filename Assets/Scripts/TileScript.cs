using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class TileScript : MonoBehaviour
{

    public Sprite Grass;
    public Sprite Sea;
    public Sprite Error;
    public Sprite[] River;
    public Sprite CoastEdge;
    public Sprite CoastCorner;
    public Sprite Mountain;
    public Sprite Ice;
    public Sprite Snow;
    public Sprite Highlight;


    public Enums.TileID tileID;

    public GameObject Feature;

    public int x;
    public int y;


    public List<GameObject> features;
    public GameObject highlightFeature;




    public void setPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.gameObject.transform.position = new Vector2( (float) (x + .5), (float) (y + .5) );
    }

    public void setID(Enums.TileID id, int info, Boolean coast)
    {
        features = new List<GameObject>();
        tileID = id;
        if ( id == Enums.TileID.grass )
        {
            this.gameObject.GetComponent<SpriteRenderer>().sprite = Grass;
        } else if ( id == Enums.TileID.sea)
        {
            this.gameObject.GetComponent<SpriteRenderer>().sprite = Sea;
        }
        else if (id == Enums.TileID.error)
        {
            this.gameObject.GetComponent<SpriteRenderer>().sprite = Error;
        
        
        
        
        } //Loads river sprite based on binary
        else if (id == Enums.TileID.river)
        {
            if (info == 1)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[0];
            }
            else if (info == 10)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[0];
                transform.Rotate(Vector3.forward * -90);
            }
            else if (info == 11)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[2];
                transform.Rotate(Vector3.forward * 180);
            } else if ( info == 100)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[0];
                transform.Rotate(Vector3.forward * 180);
            }
            else if (info == 101)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[1];
            }
            else if (info == 110)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[2];
                transform.Rotate(Vector3.forward * 90);
            }
            else if (info == 111)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[3];
                transform.Rotate(Vector3.forward * 90);
            } else if ( info == 1000)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[0];
                transform.Rotate(Vector3.forward * 90);
            }
            else if (info == 1001)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[2];
                transform.Rotate(Vector3.forward * -90);
            }
            else if (info == 1010)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[1];
                transform.Rotate(Vector3.forward * -90);
            }
            else if (info == 1011)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[3];
                transform.Rotate(Vector3.forward * 180);
            }
            else if (info == 1100)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[2];
            }
            else if (info == 1101)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[3];
                transform.Rotate(Vector3.forward * -90);
            }
            else if (info == 1110)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[3];
            }
            else if (info == 1111)
            {
                this.gameObject.GetComponent<SpriteRenderer>().sprite = River[4];
            } 
                

            //this.gameObject.GetComponent<SpriteRenderer>().sprite = River;


        }
        else if (id == Enums.TileID.mountain)
        {
            this.gameObject.GetComponent<SpriteRenderer>().sprite = Mountain;
        } else if (id == Enums.TileID.ice)
        {
            this.gameObject.GetComponent<SpriteRenderer>().sprite = Ice;
        } else if ( id == Enums.TileID.snow){
            this.gameObject.GetComponent<SpriteRenderer>().sprite = Snow;
        }


        if (coast)
        {
            setCoasts(info);
        }
    }

    /**
     * Adds a new feautre for a coast and gives it the appropriate sprite
     */
    public void setCoasts(int info)
    {
       GameObject f = Instantiate(Feature);
        f.transform.position = new Vector2((float)(x + .5), (float)(y + .5));

        if (info == 1)
            {
                f.GetComponent<SpriteRenderer>().sprite = CoastEdge;

            }
            else if (info == 10)
            {
                f.GetComponent<SpriteRenderer>().sprite = CoastEdge;
                f.transform.Rotate(Vector3.forward * -90);
            }
            else if (info == 100)
            {
                f.GetComponent<SpriteRenderer>().sprite = CoastEdge;
                f.transform.Rotate(Vector3.forward * 180);
            }
            else if (info == 1000)
            {
                f.GetComponent<SpriteRenderer>().sprite = CoastEdge;
                f.transform.Rotate(Vector3.forward * 90);
            }
            else if (info == 11)
            {
                f.GetComponent<SpriteRenderer>().sprite = CoastCorner;
            }
            else if (info == 110)
            {
                f.GetComponent<SpriteRenderer>().sprite = CoastCorner;
                f.transform.Rotate(Vector3.forward * -90);
            }
            else if (info == 1100)
            {
                f.GetComponent<SpriteRenderer>().sprite = CoastCorner;
                f.transform.Rotate(Vector3.forward * 180);
            }
            else if (info == 1001)
            {
                f.GetComponent<SpriteRenderer>().sprite = CoastCorner;
                f.transform.Rotate(Vector3.forward * 90);
            }
        features.Add(f);
        }


    void Update()
    {

    }
    void OnMouseDown()
    {
        GameObject f = Instantiate(Feature);
        f.transform.position = new Vector2((float)(x + .5), (float)(y + .5));
        f.GetComponent<SpriteRenderer>().sprite = Highlight;
        f.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .5f);
        GameObject.Find("UI Manager").GetComponent<UIScript>().setText(this.gameObject);
        highlightFeature = f;
    }

    public void deselect()
    {
        if (highlightFeature != null)
        {
            Destroy(highlightFeature);
        }
    }
}
